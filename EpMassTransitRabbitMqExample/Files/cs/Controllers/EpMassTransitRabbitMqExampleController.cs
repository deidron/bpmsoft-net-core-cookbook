namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using MassTransit.Monitoring.Health;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("ep/[controller]")]
    [Route("0/ep/[controller]")]
    [ApiController]
    [Authorize]
    public class EpMassTransitRabbitMqExampleController : ControllerBase
    {
        private const string NotInitializedMessage = "Message bus container is not initialized.";

        [HttpGet("Health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            EpBusHealthResponse health = EpMassTransitContainer.TryGetRoot(out ILifetimeScope root)
                ? root.Resolve<IEpBusHealthReporter>().GetHealth()
                : new EpBusHealthResponse
                {
                    Healthy = false,
                    Status = BusHealthStatus.Unhealthy.ToString(),
                    Description = NotInitializedMessage,
                    Endpoints = [],
                };
            if (User.Identity is not { IsAuthenticated: true })
                health = new EpBusHealthResponse { Healthy = health.Healthy, Status = health.Status };
            return StatusCode(health.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable, health);
        }

        [HttpPost("PublishGreeting")]
        public async Task<IActionResult> PublishGreeting([FromBody] EpGreetingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (!EpMassTransitContainer.TryGetRoot(out ILifetimeScope root))
                return Problem(NotInitializedMessage, statusCode: StatusCodes.Status503ServiceUnavailable);
            EpGreetingPublishResult result = await root.Resolve<IEpGreetingPublisher>()
                .PublishAsync(request.Name).ConfigureAwait(false);
            return result switch
            {
                EpGreetingPublishResult.Published =>
                    Ok(new EpGreetingResponse { Message = $"Greeting for '{request.Name}' published." }),
                EpGreetingPublishResult.NotConfirmed =>
                    Problem("Message broker did not confirm the message in time. It may still be delivered.",
                        statusCode: StatusCodes.Status503ServiceUnavailable),
                EpGreetingPublishResult.Failed =>
                    Problem("Failed to publish the message.", statusCode: StatusCodes.Status500InternalServerError),
                _ => throw new InvalidOperationException($"Unexpected publish result: {result}."),
            };
        }
    }
}

namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System.Collections.Generic;
    using MassTransit.Monitoring.Health;
    using NSubstitute;
    using Xunit;

    public sealed class EpBusHealthReporterTests
    {
        private readonly IBusHealth _busHealth = Substitute.For<IBusHealth>();

        [Fact]
        public void HealthyBusMapsToHealthyResponseWithEndpoints()
        {
            Dictionary<string, object> endpoints = new()
            {
                ["rabbitmq://localhost/vhost/EpGreeting"] = new { Message = "ready" },
            };
            Report(HealthResult.Healthy("Ready", new Dictionary<string, object> { ["Endpoints"] = endpoints }));

            EpBusHealthResponse response = new EpBusHealthReporter(_busHealth).GetHealth();

            Assert.True(response.Healthy);
            Assert.Equal("Healthy", response.Status);
            Assert.Equal("Ready", response.Description);
            Assert.Equal(["rabbitmq://localhost/vhost/EpGreeting: { Message = ready }"], response.Endpoints);
        }

        [Fact]
        public void DegradedBusIsNotHealthy()
        {
            Report(HealthResult.Degraded("Slow"));

            EpBusHealthResponse response = new EpBusHealthReporter(_busHealth).GetHealth();

            Assert.False(response.Healthy);
            Assert.Equal("Degraded", response.Status);
        }

        [Fact]
        public void UnhealthyBusKeepsDescription()
        {
            Report(HealthResult.Unhealthy("Not ready: not started"));

            EpBusHealthResponse response = new EpBusHealthReporter(_busHealth).GetHealth();

            Assert.False(response.Healthy);
            Assert.Equal("Unhealthy", response.Status);
            Assert.Equal("Not ready: not started", response.Description);
        }

        [Fact]
        public void MissingEndpointsDataGivesEmptyList()
        {
            Report(HealthResult.Healthy("Ready"));

            EpBusHealthResponse response = new EpBusHealthReporter(_busHealth).GetHealth();

            Assert.Empty(response.Endpoints);
        }

        [Fact]
        public void UnexpectedEndpointsDataGivesEmptyList()
        {
            Report(HealthResult.Healthy("Ready", new Dictionary<string, object> { ["Endpoints"] = "unexpected" }));

            EpBusHealthResponse response = new EpBusHealthReporter(_busHealth).GetHealth();

            Assert.Empty(response.Endpoints);
        }

        private void Report(HealthResult result) => _busHealth.CheckHealth().Returns(result);
    }
}

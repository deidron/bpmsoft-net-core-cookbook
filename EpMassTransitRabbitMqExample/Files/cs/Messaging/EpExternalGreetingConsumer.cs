namespace EP.EpMassTransitRabbitMqExample
{
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpExternalGreetingConsumer : IConsumer<EpExternalGreeting>
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpExternalGreetingConsumer));

        public Task Consume(ConsumeContext<EpExternalGreeting> context)
        {
            s_log.Info($"External hello, {context.Message.Name}! (content type {context.ReceiveContext.ContentType})");
            return Task.CompletedTask;
        }
    }
}

namespace EP.EpMassTransitRabbitMqExample
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpGreetingFaultConsumer : IConsumer<Fault<EpGreetingRequested>>
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpGreetingFaultConsumer));

        public Task Consume(ConsumeContext<Fault<EpGreetingRequested>> context)
        {
            Fault<EpGreetingRequested> fault = context.Message;
            ExceptionInfo exception = fault.Exceptions.FirstOrDefault();
            s_log.Warn($"Greeting for '{fault.Message.Name}' failed on {fault.Host?.MachineName}: " +
                $"{exception?.ExceptionType}: {exception?.Message} (FaultedMessageId {fault.FaultedMessageId})");
            return Task.CompletedTask;
        }
    }
}

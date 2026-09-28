namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpGreetingConsumer : IConsumer<EpGreetingRequested>
    {
        internal const string DemoTransientFailureName = "fail";

        internal const string DemoInvalidDataName = "invalid";

        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpGreetingConsumer));

        public Task Consume(ConsumeContext<EpGreetingRequested> context)
        {
            string name = context.Message.Name;
            if (string.Equals(name, DemoTransientFailureName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Demo transient failure for '{name}'.");
            if (string.Equals(name, DemoInvalidDataName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Demo invalid data: '{name}'.", nameof(context));

            s_log.Info($"Hello, {name}! (MessageId {context.MessageId})");
            return Task.CompletedTask;
        }
    }
}

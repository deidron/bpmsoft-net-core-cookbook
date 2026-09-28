namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpReceiveLogObserver : IReceiveObserver
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpReceiveLogObserver));

        public Task PreReceive(ReceiveContext context) => Task.CompletedTask;

        public Task PostReceive(ReceiveContext context) => Task.CompletedTask;

        public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
            where T : class => Task.CompletedTask;

        public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
            where T : class
        {
            s_log.Error($"[MassTransit] Consumer {consumerType} failed on {typeof(T).Name}, MessageId {context.MessageId}.", exception);
            return Task.CompletedTask;
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            s_log.Error($"[MassTransit] Receive fault on {context.InputAddress}.", exception);
            return Task.CompletedTask;
        }
    }
}

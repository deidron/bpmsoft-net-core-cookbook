namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpSendLogObserver : ISendObserver
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpSendLogObserver));

        public Task PreSend<T>(SendContext<T> context)
            where T : class => Task.CompletedTask;

        public Task PostSend<T>(SendContext<T> context)
            where T : class => Task.CompletedTask;

        public Task SendFault<T>(SendContext<T> context, Exception exception)
            where T : class
        {
            s_log.Error($"[MassTransit] Send of {typeof(T).Name} to {context.DestinationAddress} failed, MessageId {context.MessageId}.", exception);
            return Task.CompletedTask;
        }
    }
}

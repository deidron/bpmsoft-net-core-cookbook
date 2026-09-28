namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpPublishLogObserver : IPublishObserver
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpPublishLogObserver));

        public Task PrePublish<T>(PublishContext<T> context)
            where T : class => Task.CompletedTask;

        public Task PostPublish<T>(PublishContext<T> context)
            where T : class => Task.CompletedTask;

        public Task PublishFault<T>(PublishContext<T> context, Exception exception)
            where T : class
        {
            s_log.Error($"[MassTransit] Publish of {typeof(T).Name} to {context.DestinationAddress} failed, MessageId {context.MessageId}.", exception);
            return Task.CompletedTask;
        }
    }
}

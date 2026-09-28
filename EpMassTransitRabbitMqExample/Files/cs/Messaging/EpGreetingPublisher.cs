namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpGreetingPublisher : IEpGreetingPublisher
    {
        private static readonly TimeSpan s_defaultPublishTimeout = TimeSpan.FromSeconds(10);

        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpGreetingPublisher));

        private readonly IBus _bus;

        private readonly TimeSpan _publishTimeout;

        public EpGreetingPublisher(IBus bus)
            : this(bus, s_defaultPublishTimeout)
        {
        }

        internal EpGreetingPublisher(IBus bus, TimeSpan publishTimeout)
        {
            _bus = bus;
            _publishTimeout = publishTimeout;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "The failure is logged here and reported to the caller as a result.")]
        public async Task<EpGreetingPublishResult> PublishAsync(string name)
        {
            using CancellationTokenSource timeout = new(_publishTimeout);
            try
            {
                await _bus.Publish(new EpGreetingRequested { Name = name }, timeout.Token).ConfigureAwait(false);
                return EpGreetingPublishResult.Published;
            }
            catch (OperationCanceledException ex) when (timeout.IsCancellationRequested)
            {
                s_log.Warn($"[MassTransit] Greeting for '{name}': message broker did not confirm the message within {_publishTimeout}. It may still be delivered.", ex);
                return EpGreetingPublishResult.NotConfirmed;
            }
            catch (Exception ex)
            {
                s_log.Error($"[MassTransit] Greeting for '{name}': failed to publish the message.", ex);
                return EpGreetingPublishResult.Failed;
            }
        }
    }
}

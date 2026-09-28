namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;
    using NSubstitute;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    public sealed class EpGreetingPublisherTests
    {
        private static readonly TimeSpan s_publishTimeout = TimeSpan.FromMilliseconds(100);

        private static readonly Type s_source = typeof(EpGreetingPublisher);

        private readonly IBus _bus = Substitute.For<IBus>();

        private readonly EpGreetingPublisher _publisher;

        public EpGreetingPublisherTests()
        {
            EpLogCapture.Clear();
            _publisher = new EpGreetingPublisher(_bus, s_publishTimeout);
        }

        [Fact]
        public async Task PublishedGreetingCarriesName()
        {
            OnPublish(_ => Task.CompletedTask);

            EpGreetingPublishResult result = await _publisher.PublishAsync("Alice").ConfigureAwait(true);

            Assert.Equal(EpGreetingPublishResult.Published, result);
            await _bus.Received(1).Publish(
                Arg.Is<EpGreetingRequested>(message => message.Name == "Alice"),
                Arg.Any<CancellationToken>()).ConfigureAwait(true);
        }

        [Fact]
        public async Task TimeoutReturnsNotConfirmedWithWarning()
        {
            OnPublish(token => Task.Delay(Timeout.Infinite, token));

            EpGreetingPublishResult result = await _publisher.PublishAsync("slow").ConfigureAwait(true);

            Assert.Equal(EpGreetingPublishResult.NotConfirmed, result);
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Warn, $"did not confirm the message within {s_publishTimeout}"));
            Assert.Empty(EpLogCapture.Events(s_source, LogLevel.Error));
        }

        [Fact]
        public async Task PublishExceptionReturnsFailedWithError()
        {
            OnPublish(_ => Task.FromException(new InvalidOperationException("Broker failure.")));

            EpGreetingPublishResult result = await _publisher.PublishAsync("broken").ConfigureAwait(true);

            Assert.Equal(EpGreetingPublishResult.Failed, result);
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Error, "failed to publish the message"));
        }

        [Fact]
        public async Task SynchronousPublishExceptionReturnsFailed()
        {
            OnPublish(_ => throw new InvalidOperationException("Broker failure."));

            EpGreetingPublishResult result = await _publisher.PublishAsync("broken").ConfigureAwait(true);

            Assert.Equal(EpGreetingPublishResult.Failed, result);
        }

        [Fact]
        public async Task CancellationNotCausedByTimeoutIsFailure()
        {
            OnPublish(_ => Task.FromException(new OperationCanceledException()));

            EpGreetingPublishResult result = await _publisher.PublishAsync("cancelled").ConfigureAwait(true);

            Assert.Equal(EpGreetingPublishResult.Failed, result);
            Assert.Empty(EpLogCapture.Events(s_source, LogLevel.Warn));
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Error, "failed to publish the message"));
        }

        private void OnPublish(Func<CancellationToken, Task> publish) =>
            _bus.Publish(Arg.Any<EpGreetingRequested>(), Arg.Any<CancellationToken>())
                .Returns(call => publish(call.Arg<CancellationToken>()));
    }
}

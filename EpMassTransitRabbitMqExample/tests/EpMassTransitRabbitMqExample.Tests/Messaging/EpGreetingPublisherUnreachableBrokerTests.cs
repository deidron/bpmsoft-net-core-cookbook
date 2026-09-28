namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Autofac;
    using Common.Logging;
    using MassTransit;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    public sealed class EpGreetingPublisherUnreachableBrokerTests
    {
        private static readonly TimeSpan s_publishTimeout = TimeSpan.FromMilliseconds(500);

        public EpGreetingPublisherUnreachableBrokerTests() => EpLogCapture.Clear();

        [Fact]
        public async Task PublishWithUnreachableBrokerIsNotConfirmed()
        {
            using IContainer container = EpMassTransitRabbitMqInitializer.CreateContainer(
                new Uri("amqp://user:secret@localhost:1/vhost"));
            EpGreetingPublisher publisher = new(container.Resolve<IBus>(), s_publishTimeout);
            Stopwatch stopwatch = Stopwatch.StartNew();

            EpGreetingPublishResult result = await publisher.PublishAsync("offline").ConfigureAwait(true);

            Assert.Equal(EpGreetingPublishResult.NotConfirmed, result);
            Assert.InRange(stopwatch.Elapsed, s_publishTimeout, TimeSpan.FromSeconds(5));
            Assert.Equal(1, EpLogCapture.Count(typeof(EpGreetingPublisher), LogLevel.Warn, "did not confirm"));
        }
    }
}

namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using Autofac;
    using Common.Logging;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    public sealed class EpMassTransitRabbitMqInitializerTests : IDisposable
    {
        private static readonly Uri s_unreachableBroker = new("amqp://user:secret@localhost:1/vhost");

        private static readonly Type s_source = typeof(EpMassTransitRabbitMqInitializer);

        public EpMassTransitRabbitMqInitializerTests()
        {
            EpLogCapture.Clear();
            EpMassTransitContainer.Shutdown();
        }

        public void Dispose() => EpMassTransitContainer.Shutdown();

        [Fact]
        public void InitializeBuildsContainerAndLogsAddressWithoutCredentials()
        {
            EpMassTransitRabbitMqInitializer.Initialize(s_unreachableBroker);

            Assert.True(EpMassTransitContainer.TryGetRoot(out _));
            string message = Assert.Single(EpLogCapture.Events(s_source, LogLevel.Info)).RenderedMessage;
            Assert.Contains("amqp://localhost:1/vhost", message, StringComparison.Ordinal);
            Assert.DoesNotContain("secret", message, StringComparison.Ordinal);
            Assert.DoesNotContain("user", message, StringComparison.Ordinal);
        }

        [Fact]
        public void SecondInitializeWarnsAndKeepsContainer()
        {
            EpMassTransitRabbitMqInitializer.Initialize(s_unreachableBroker);
            Assert.True(EpMassTransitContainer.TryGetRoot(out ILifetimeScope first));

            EpMassTransitRabbitMqInitializer.Initialize(s_unreachableBroker);

            Assert.True(EpMassTransitContainer.TryGetRoot(out ILifetimeScope second));
            Assert.Same(first, second);
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Warn, "already initialized"));
        }

        [Fact]
        public void StopShutsDownContainer()
        {
            EpMassTransitRabbitMqInitializer.Initialize(s_unreachableBroker);

            new EpMassTransitRabbitMqInitializer().Stop();

            Assert.False(EpMassTransitContainer.TryGetRoot(out _));
        }

        [Fact]
        public void InitializeRejectsNullAddress() =>
            Assert.Throws<ArgumentNullException>(() => EpMassTransitRabbitMqInitializer.Initialize(null));
    }
}

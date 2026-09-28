namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using Autofac;
    using GreenPipes;
    using MassTransit;
    using Newtonsoft.Json;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    public sealed class EpServiceCollectionExtensionsTests
    {
        public EpServiceCollectionExtensionsTests() => EpLogCapture.Clear();

        [Theory]
        [InlineData("amqp://user:secret@localhost:1/vhost", "00:00:10")]
        [InlineData("amqp://user:secret@localhost:1/vhost?heartbeat=20", "00:00:20")]
        public void HeartbeatComesFromAddressOrDefaultsToTenSeconds(string address, string expected)
        {
            using IContainer container = EpMassTransitRabbitMqInitializer.CreateContainer(new Uri(address));

            string probe = JsonConvert.SerializeObject(container.Resolve<IBusControl>().GetProbeResult());

            Assert.Contains($"\"Heartbeat\":\"{expected}\"", probe, StringComparison.Ordinal);
            Assert.DoesNotContain("secret", probe, StringComparison.Ordinal);
        }
    }
}

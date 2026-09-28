namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using Xunit;

    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class EpStaticContainerGroup
    {
        public const string Name = "EpMassTransitContainer";
    }
}

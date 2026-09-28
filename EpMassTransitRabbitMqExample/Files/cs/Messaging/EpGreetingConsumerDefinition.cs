namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;

    internal sealed class EpGreetingConsumerDefinition : ConsumerDefinition<EpGreetingConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<EpGreetingConsumer> consumerConfigurator) =>
            consumerConfigurator.UseMessageRetry(retry =>
            {
                retry.Ignore<ArgumentException>();
                _ = retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
            });
    }
}

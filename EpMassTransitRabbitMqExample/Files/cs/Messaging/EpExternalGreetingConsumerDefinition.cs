namespace EP.EpMassTransitRabbitMqExample
{
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;

    internal sealed class EpExternalGreetingConsumerDefinition : ConsumerDefinition<EpExternalGreetingConsumer>
    {
        internal const string QueueName = "ep-external-greeting";

        public EpExternalGreetingConsumerDefinition() => EndpointName = QueueName;

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<EpExternalGreetingConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseRawJsonSerializer();
            endpointConfigurator.ConfigureConsumeTopology = false;
        }
    }
}

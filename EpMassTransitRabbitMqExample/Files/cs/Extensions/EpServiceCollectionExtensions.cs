namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal static class EpServiceCollectionExtensions
    {
        private const ushort DefaultHeartbeatSeconds = 10;

        public static IServiceCollection AddMassTransitRabbitMq(this IServiceCollection services, Uri messageBrokerAddress)
        {
            ArgumentNullException.ThrowIfNull(messageBrokerAddress);
            _ = services.AddSingleton<ILoggerFactory>(
                _ => new EpMassTransitLoggerFactory(typeof(EpServiceCollectionExtensions).Namespace));
            _ = services.AddMassTransitLogObservers();
            return services.AddMassTransit(x =>
            {
                _ = x.AddConsumer<EpGreetingConsumer>(typeof(EpGreetingConsumerDefinition));
                _ = x.AddConsumer<EpGreetingFaultConsumer>();
                _ = x.AddConsumer<EpExternalGreetingConsumer>(typeof(EpExternalGreetingConsumerDefinition));
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(messageBrokerAddress, h =>
                    {
                        if (new RabbitMqHostAddress(messageBrokerAddress).Heartbeat is null)
                            h.Heartbeat(DefaultHeartbeatSeconds);
                    });
                    cfg.ConnectMassTransitLogObservers(context);
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}

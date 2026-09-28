namespace EP.EpMassTransitRabbitMqExample
{
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;

    internal static class EpLogObserverExtensions
    {
        internal static IServiceCollection AddMassTransitLogObservers(this IServiceCollection services) =>
            services
                .AddSingleton<EpBusLogObserver>()
                .AddSingleton<EpReceiveEndpointLogObserver>()
                .AddSingleton<EpReceiveLogObserver>()
                .AddSingleton<EpPublishLogObserver>()
                .AddSingleton<EpSendLogObserver>();

        internal static void ConnectMassTransitLogObservers(this IBusFactoryConfigurator configurator, IBusRegistrationContext context)
        {
            _ = configurator.ConnectBusObserver(context.GetRequiredService<EpBusLogObserver>());
            _ = configurator.ConnectReceiveObserver(context.GetRequiredService<EpReceiveLogObserver>());
            _ = configurator.ConnectPublishObserver(context.GetRequiredService<EpPublishLogObserver>());
            _ = configurator.ConnectSendObserver(context.GetRequiredService<EpSendLogObserver>());
        }
    }
}

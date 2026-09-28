namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Reflection;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using BPMSoft.Core;
    using BPMSoft.Core.Factories;
    using Common.Logging;
    using EP.EpMassTransitRabbitMqExample.Api;
    using Microsoft.Extensions.DependencyInjection;

    [DefaultBinding(typeof(IEpMassTransitRabbitMqInitializer))]
    internal class EpMassTransitRabbitMqInitializer : IEpMassTransitRabbitMqInitializer
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpMassTransitRabbitMqInitializer));

        public void Initialize(AppConnection appConnection)
        {
            ArgumentNullException.ThrowIfNull(appConnection);
            Initialize(appConnection.GetRabbitMqConnectionUri());
        }

        internal static void Initialize(Uri messageBrokerAddress)
        {
            ArgumentNullException.ThrowIfNull(messageBrokerAddress);
            if (!EpMassTransitContainer.TryInitialize(() => CreateContainer(messageBrokerAddress)))
            {
                s_log.Warn("Message bus container is already initialized. Nothing was built.");
                return;
            }
            s_log.Info($"Message bus container built. Connecting to {messageBrokerAddress.ToLogString()} in the background.");
        }

        public void Stop() => EpMassTransitContainer.Shutdown();

        internal static IContainer CreateContainer(Uri messageBrokerAddress)
        {
            ServiceCollection services = new();
            _ = services.AddMassTransitRabbitMq(messageBrokerAddress);
            ContainerBuilder containerBuilder = new();
            containerBuilder.Populate(services);
            Assembly assembly = typeof(EpMassTransitRabbitMqInitializer).Assembly;
            _ = containerBuilder.RegisterAssemblyModules(assembly);
            return containerBuilder.Build();
        }
    }
}

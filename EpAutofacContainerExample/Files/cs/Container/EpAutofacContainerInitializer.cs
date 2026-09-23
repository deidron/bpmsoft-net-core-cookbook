namespace EP.EpAutofacContainerExample
{
    using System.Reflection;
    using Autofac;
    using BPMSoft.Core.Factories;
    using Common.Logging;
    using EP.EpAutofacContainerExample.Api;

    [DefaultBinding(typeof(IEpAutofacContainerInitializer))]
    internal class EpAutofacContainerInitializer : IEpAutofacContainerInitializer
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpAutofacContainerInitializer));

        public void Initialize()
        {
            if (!EpAutofacContainer.TryInitialize(CreateContainer))
                s_log.Warn("Autofac Container is already initialized. Nothing was built.");
        }

        public void Stop() => EpAutofacContainer.Shutdown();

        private static IContainer CreateContainer()
        {
            ContainerBuilder containerBuilder = new();
            Assembly assembly = typeof(EpAutofacContainerInitializer).Assembly;
            _ = containerBuilder.RegisterAssemblyModules(assembly);
            return containerBuilder.Build();
        }
    }
}

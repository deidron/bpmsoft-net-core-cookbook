namespace EP.EpNInjectContainerExample
{
    using BPMSoft.Core.Factories;
    using Common.Logging;
    using EP.EpNInjectContainerExample.Api;
    using Ninject;
    using Ninject.Extensions.Factory;

    [DefaultBinding(typeof(IEpNInjectContainerInitializer))]
    internal class EpNInjectContainerInitializer : IEpNInjectContainerInitializer
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpNInjectContainerInitializer));

        public void Initialize()
        {
            if (!EpNInjectContainer.TryInitialize(CreateKernel))
                s_log.Warn("Ninject Kernel is already initialized. Nothing was built.");
        }

        public void Stop() => EpNInjectContainer.Shutdown();

        private static StandardKernel CreateKernel()
        {
            NinjectSettings settings = new() { LoadExtensions = false };
            StandardKernel kernel = new(settings, new FuncModule());
            kernel.Load(typeof(EpNInjectContainerInitializer).Assembly);
            return kernel;
        }
    }
}

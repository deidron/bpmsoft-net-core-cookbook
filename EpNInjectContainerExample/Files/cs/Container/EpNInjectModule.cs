namespace EP.EpNInjectContainerExample
{
    using BPMSoft.Core;
    using Common.Logging;
    using Ninject.Extensions.Factory;
    using Ninject.Modules;

    /// <summary>
    /// Bindings of the package. Must stay <c>public</c>: <c>IKernel.Load(Assembly)</c> discovers
    /// modules through <c>Assembly.ExportedTypes</c>, so an <c>internal</c> module is silently
    /// skipped: the kernel builds without error and the missing bindings surface only on resolution.
    /// </summary>
    public class EpNInjectModule : NinjectModule
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpNInjectModule));

        public override void Load()
        {
            s_log.Debug($"{nameof(EpNInjectModule)}: loading bindings.");
            _ = Bind<IEpNotificationFormatter>().To<EpPlainTextNotificationFormatter>().InSingletonScope();
            _ = Bind<IEpNotificationFormatter>().To<EpHtmlNotificationFormatter>()
                .WhenInjectedInto<EpEmailNotificationChannel>().InSingletonScope();
            _ = Bind<IEpNotificationChannel>().To<EpLogNotificationChannel>().InSingletonScope();
            _ = Bind<IEpNotificationChannel>().To<EpFeedNotificationChannel>().InSingletonScope();
            _ = Bind<IEpNotificationChannel>().To<EpEmailNotificationChannel>().InSingletonScope();
            _ = Bind<UserConnection>().ToMethod(context => context.GetUserConnectionScope().UserConnection);
            _ = Bind<IEpNotifier>().To<EpNotifier>().InTransientScope();
            _ = Bind<IEpNotifierFactory>().ToFactory();
            _ = Bind<IEpCurrentUserContext>().To<EpCurrentUserContext>().InUserConnectionScope();
            _ = Bind<IEpPersonalGreeter>().To<EpPersonalGreeter>().InTransientScope();
            _ = Bind<IEpUserCard>().To<EpUserCard>().InTransientScope();
        }
    }
}

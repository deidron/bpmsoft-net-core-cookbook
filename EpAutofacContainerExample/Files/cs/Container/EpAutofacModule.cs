namespace EP.EpAutofacContainerExample
{
    using Autofac;
    using Common.Logging;

    internal class EpAutofacModule : Module
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpAutofacModule));

        protected override void Load(ContainerBuilder builder)
        {
            s_log.Debug($"{nameof(EpAutofacModule)} : loading bindings.");
            _ = builder.RegisterType<EpSystemClock>().As<IEpClock>().SingleInstance();
            _ = builder.RegisterType<EpGreetingService>().As<IEpGreetingService>().SingleInstance();
            _ = builder.RegisterType<EpCurrentUserGreeter>().As<IEpCurrentUserGreeter>().InstancePerDependency();
            _ = builder.RegisterType<EpCurrentUserContext>().As<IEpCurrentUserContext>().InstancePerUserConnection();
            _ = builder.RegisterType<EpPersonalGreeter>().As<IEpPersonalGreeter>().InstancePerDependency();
            _ = builder.RegisterType<EpUserCard>().As<IEpUserCard>().InstancePerDependency();
        }
    }
}

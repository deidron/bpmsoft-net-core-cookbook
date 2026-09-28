namespace EP.EpMassTransitRabbitMqExample
{
    using Autofac;
    using Common.Logging;

    internal class EpMassTransitModule : Module
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpMassTransitModule));

        protected override void Load(ContainerBuilder builder)
        {
            s_log.Debug($"{nameof(EpMassTransitModule)} : loading bindings.");
            _ = builder.RegisterType<EpMassTransitLifetimeManager>().As<IStartable>().SingleInstance();
            _ = builder.RegisterType<EpGreetingPublisher>().As<IEpGreetingPublisher>().SingleInstance();
            _ = builder.RegisterType<EpBusHealthReporter>().As<IEpBusHealthReporter>().SingleInstance();
        }
    }
}

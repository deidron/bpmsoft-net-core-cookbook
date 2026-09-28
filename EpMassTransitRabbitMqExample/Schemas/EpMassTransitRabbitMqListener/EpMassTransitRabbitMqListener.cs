namespace BPMSoft.Configuration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BPMSoft.Core;
    using BPMSoft.Core.Factories;
    using BPMSoft.Web.Common;
    using global::Common.Logging;
    using global::EP.EpMassTransitRabbitMqExample.Api;

    public class EpMassTransitRabbitMqListener : AppEventListenerBase
    {
        private static readonly ILog s_logger = LogManager.GetLogger(typeof(EpMassTransitRabbitMqListener));

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "An exception would break application start. The failure is logged and the application keeps running without the bus.")]
        public override void OnAppStart(AppEventContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            base.OnAppStart(context);
            s_logger.Info("MassTransit/RabbitMQ integration: initializing.");
            try
            {
                AppConnection appConnection = context.Application["AppConnection"] as AppConnection;
                IEpMassTransitRabbitMqInitializer initializer = ClassFactory.Get<IEpMassTransitRabbitMqInitializer>();
                initializer.Initialize(appConnection);
            }
            catch (Exception ex)
            {
                s_logger.Error("MassTransit/RabbitMQ integration: failed to initialize.", ex);
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "An exception would break application shutdown and the remaining listeners. The failure is logged.")]
        public override void OnAppEnd(AppEventContext context)
        {
            base.OnAppEnd(context);
            s_logger.Info("MassTransit/RabbitMQ integration: stopping.");
            try
            {
                IEpMassTransitRabbitMqInitializer initializer = ClassFactory.Get<IEpMassTransitRabbitMqInitializer>();
                initializer.Stop();
            }
            catch (Exception ex)
            {
                s_logger.Error("MassTransit/RabbitMQ integration: failed to stop.", ex);
            }
        }
    }
}

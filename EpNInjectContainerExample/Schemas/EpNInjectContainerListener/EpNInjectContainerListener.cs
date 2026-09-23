namespace BPMSoft.Configuration
{
    using System;
    using BPMSoft.Core.Factories;
    using BPMSoft.Web.Common;
    using global::Common.Logging;
    using global::EP.EpNInjectContainerExample.Api;

    public class EpNInjectContainerListener : AppEventListenerBase
    {
        private static readonly ILog s_logger = LogManager.GetLogger(typeof(EpNInjectContainerListener));

        public override void OnAppStart(AppEventContext context)
        {
            base.OnAppStart(context);
            try
            {
                IEpNInjectContainerInitializer initializer = ClassFactory.Get<IEpNInjectContainerInitializer>();
                initializer.Initialize();
                s_logger.Info($"NInjectContainer started at {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                s_logger.Error("NInjectContainer failed to start", ex);
                throw;
            }
        }

        public override void OnAppEnd(AppEventContext context)
        {
            base.OnAppEnd(context);
            s_logger.Info($"NInjectContainer stopping at {DateTime.UtcNow}");
            try
            {
                IEpNInjectContainerInitializer initializer = ClassFactory.Get<IEpNInjectContainerInitializer>();
                initializer.Stop();
                s_logger.Info("NInjectContainer stopped successfully");
            }
            catch (Exception ex)
            {
                s_logger.Error("NInjectContainer failed to stop gracefully", ex);
                throw;
            }
        }
    }
}

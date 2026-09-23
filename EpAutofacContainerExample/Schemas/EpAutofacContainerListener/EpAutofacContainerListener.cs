namespace BPMSoft.Configuration
{
    using System;
    using BPMSoft.Core.Factories;
    using BPMSoft.Web.Common;
    using global::Common.Logging;
    using global::EP.EpAutofacContainerExample.Api;

    public class EpAutofacContainerListener : AppEventListenerBase
    {
        private static readonly ILog s_logger = LogManager.GetLogger(typeof(EpAutofacContainerListener));

        public override void OnAppStart(AppEventContext context)
        {
            base.OnAppStart(context);
            try
            {
                IEpAutofacContainerInitializer initializer = ClassFactory.Get<IEpAutofacContainerInitializer>();
                initializer.Initialize();
                s_logger.Info($"AutofacContainer started at {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                s_logger.Error("AutofacContainer failed to start", ex);
                throw;
            }
        }

        public override void OnAppEnd(AppEventContext context)
        {
            base.OnAppEnd(context);
            s_logger.Info($"AutofacContainer stopping at {DateTime.UtcNow}");
            try
            {
                IEpAutofacContainerInitializer initializer = ClassFactory.Get<IEpAutofacContainerInitializer>();
                initializer.Stop();
                s_logger.Info("AutofacContainer stopped successfully");
            }
            catch (Exception ex)
            {
                s_logger.Error("AutofacContainer failed to stop gracefully", ex);
                throw;
            }
        }
    }
}

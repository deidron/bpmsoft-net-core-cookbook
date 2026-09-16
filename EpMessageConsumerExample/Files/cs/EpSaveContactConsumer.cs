namespace EP.EpMessageConsumerExample
{
    using System;
    using BPMSoft.Core;
    using BPMSoft.Core.Entities;
    using BPMSoft.Core.Factories;
    using BPMSoft.ServiceBus;
    using Common.Logging;

    [DefaultBinding(typeof(EpSaveContactConsumer))]
    public sealed class EpSaveContactConsumer : IMessageConsumer<EpSaveContactCommand>
    {
        private static readonly ILog s_logger = LogManager.GetLogger(nameof(EpSaveContactConsumer));

        public void Consume(EpSaveContactCommand message, IConsumingContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            UserConnection userConnection = context.UserConnection;
            try
            {
                Entity entity = Entity.DeserializeFromJson(userConnection, message.JsonData);
                entity.SetDefColumnValues();
                _ = entity.Save(validateRequired: false, setColumnDefValue: false);
                if (s_logger.IsDebugEnabled)
                    s_logger.Debug($"Contact {entity.PrimaryColumnValue} saved");
            }
            catch (Exception ex)
            {
                s_logger.Error("Failed to save contact from the incoming message", ex);
                throw;
            }
        }
    }
}

namespace EP.EpMessageConsumerExample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BPMSoft.Core.Entities;
    using BPMSoft.Core.Factories;
    using BPMSoft.ServiceBus;
    using BPMStudio.Messaging.MessageBus;
    using Common.Logging;

    [DefaultBinding(typeof(EpGenerateContactConsumer))]
    public sealed class EpGenerateContactConsumer : IMessageConsumer<EpGenerateContactCommand>
    {
        private readonly IEpContactFakeDataGenerator _contactFakeDataGenerator;

        private static readonly ILog s_logger = LogManager.GetLogger(nameof(EpGenerateContactConsumer));

        private static readonly EntitySerializeOptions s_serializeOptions =
            new EntitySerializeOptions(writeDefaultValues: true, isIsoDateFormat: true);

        public EpGenerateContactConsumer(IEpContactFakeDataGenerator contactFakeDataGenerator) => _contactFakeDataGenerator = contactFakeDataGenerator;

        public void Consume(EpGenerateContactCommand message, IConsumingContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (message.Count > EpGenerateContactCommand.MaxCount)
            {
                throw new ArgumentOutOfRangeException(nameof(message), message.Count,
                    $"Count must not exceed {EpGenerateContactCommand.MaxCount}.");
            }

            IReadOnlyList<Entity> entities =
                _contactFakeDataGenerator.Generate(context.UserConnection, message.Count);
            foreach (Entity entity in entities)
                SendSaveContactCommand(context.Bus, entity);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Logged and skipped so that the rest of the batch still gets written.")]
        private static void SendSaveContactCommand(IBus bus, Entity entity)
        {
            try
            {
                string jsonData = Entity.SerializeToJson(entity, s_serializeOptions);
                bus.SendToIncoming(new EpSaveContactCommand { JsonData = jsonData });
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to send the save command for contact {entity.PrimaryColumnValue}", ex);
            }
        }
    }
}

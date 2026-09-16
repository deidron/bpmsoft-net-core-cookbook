namespace EP.EpMessageConsumerExample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BPMSoft.Core;
    using BPMSoft.Core.Entities;
    using BPMSoft.Core.Factories;
    using BPMSoft.Core.Tasks;
    using Common.Logging;

    /// <summary>
    /// The same job as <see cref="EpGenerateContactConsumer"/> plus <see cref="EpSaveContactConsumer"/>,
    /// done without the message bus. Compare the two: this one runs as a single unit of work on the
    /// actor pool of the task runner, so it never competes with the platform's business processes -
    /// but it lives only in memory, so an application restart loses it without a trace.
    /// </summary>
    [DefaultBinding(typeof(EpGenerateContactsBackgroundTask))]
    public sealed class EpGenerateContactsBackgroundTask
        : IBackgroundTask<EpGenerateContactsTaskParameters>, IUserConnectionRequired
    {
        private static readonly ILog s_logger = LogManager.GetLogger(nameof(EpGenerateContactsBackgroundTask));

        private readonly IEpContactFakeDataGenerator _contactFakeDataGenerator;

        private UserConnection _userConnection;

        public EpGenerateContactsBackgroundTask(IEpContactFakeDataGenerator contactFakeDataGenerator) =>
            _contactFakeDataGenerator = contactFakeDataGenerator;

        public void SetUserConnection(UserConnection userConnection) => _userConnection = userConnection;

        public void Run(EpGenerateContactsTaskParameters parameters)
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (parameters.Count > EpGenerateContactsTaskParameters.MaxCount)
            {
                throw new ArgumentOutOfRangeException(nameof(parameters), parameters.Count,
                    $"Count must not exceed {EpGenerateContactsTaskParameters.MaxCount}.");
            }
            IReadOnlyList<Entity> entities =
                _contactFakeDataGenerator.Generate(_userConnection, parameters.Count);
            foreach (Entity entity in entities)
                SaveContact(entity);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Logged and skipped so that the rest of the batch still gets written.")]
        private static void SaveContact(Entity entity)
        {
            try
            {
                entity.SetDefColumnValues();
                _ = entity.Save(validateRequired: false, setColumnDefValue: false);
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to save contact {entity.PrimaryColumnValue}", ex);
            }
        }
    }
}

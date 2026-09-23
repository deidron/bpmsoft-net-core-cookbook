namespace EP.EpNInjectContainerExample
{
    using System;
    using BPMSoft.Core;
    using Common.Logging;

    internal sealed class EpCurrentUserContext : IEpCurrentUserContext, IDisposable
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpCurrentUserContext));

        public EpCurrentUserContext(UserConnection userConnection)
        {
            ArgumentNullException.ThrowIfNull(userConnection);
            UserName = userConnection.CurrentUser.Name;
            ContactId = userConnection.CurrentUser.ContactId;
            s_log.Debug($"User context {ContextId} created.");
        }

        public Guid ContextId { get; } = Guid.NewGuid();

        public string UserName { get; }

        public Guid ContactId { get; }

        public void Dispose() => s_log.Debug($"User context {ContextId} released.");
    }
}

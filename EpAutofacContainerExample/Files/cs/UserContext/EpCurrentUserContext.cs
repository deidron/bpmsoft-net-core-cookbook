namespace EP.EpAutofacContainerExample
{
    using System;
    using BPMSoft.Core;

    internal sealed class EpCurrentUserContext : IEpCurrentUserContext
    {
        public EpCurrentUserContext(UserConnection userConnection)
        {
            ArgumentNullException.ThrowIfNull(userConnection);
            UserName = userConnection.CurrentUser.Name;
            ContactId = userConnection.CurrentUser.ContactId;
        }

        public Guid ContextId { get; } = Guid.NewGuid();

        public string UserName { get; }

        public Guid ContactId { get; }
    }
}

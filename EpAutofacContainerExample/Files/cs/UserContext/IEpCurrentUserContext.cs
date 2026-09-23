namespace EP.EpAutofacContainerExample
{
    using System;

    public interface IEpCurrentUserContext
    {
        Guid ContextId { get; }

        string UserName { get; }

        Guid ContactId { get; }
    }
}

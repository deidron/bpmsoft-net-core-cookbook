namespace EP.EpNInjectContainerExample
{
    internal sealed class EpUserCard(IEpCurrentUserContext context) : IEpUserCard
    {
        public IEpCurrentUserContext Context => context;

        public string Describe() => $"{context.UserName} (contact {context.ContactId})";
    }
}

namespace EP.EpNInjectContainerExample
{
    public interface IEpUserCard
    {
        IEpCurrentUserContext Context { get; }

        string Describe();
    }
}

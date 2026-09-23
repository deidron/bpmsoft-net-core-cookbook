namespace EP.EpAutofacContainerExample
{
    public interface IEpUserCard
    {
        IEpCurrentUserContext Context { get; }

        string Describe();
    }
}

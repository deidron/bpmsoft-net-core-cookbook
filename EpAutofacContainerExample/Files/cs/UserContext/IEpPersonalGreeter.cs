namespace EP.EpAutofacContainerExample
{
    public interface IEpPersonalGreeter
    {
        IEpCurrentUserContext Context { get; }

        string Greet();
    }
}

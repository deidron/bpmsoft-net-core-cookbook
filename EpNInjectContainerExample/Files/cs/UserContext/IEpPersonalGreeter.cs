namespace EP.EpNInjectContainerExample
{
    public interface IEpPersonalGreeter
    {
        IEpCurrentUserContext Context { get; }

        string Greet();
    }
}

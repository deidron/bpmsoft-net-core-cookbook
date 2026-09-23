namespace EP.EpNInjectContainerExample
{
    internal sealed class EpPersonalGreeter(IEpCurrentUserContext context) : IEpPersonalGreeter
    {
        public IEpCurrentUserContext Context => context;

        public string Greet() => $"Hello, {context.UserName}!";
    }
}

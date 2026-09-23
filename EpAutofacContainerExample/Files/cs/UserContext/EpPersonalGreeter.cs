namespace EP.EpAutofacContainerExample
{
    internal sealed class EpPersonalGreeter(IEpCurrentUserContext context, IEpGreetingService greetingService)
        : IEpPersonalGreeter
    {
        public IEpCurrentUserContext Context => context;

        public string Greet() => greetingService.Greet(context.UserName);
    }
}

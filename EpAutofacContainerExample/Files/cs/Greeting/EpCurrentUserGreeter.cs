namespace EP.EpAutofacContainerExample
{
    using BPMSoft.Core;

    internal sealed class EpCurrentUserGreeter(UserConnection userConnection, IEpGreetingService greetingService)
        : IEpCurrentUserGreeter
    {
        public string GreetCurrentUser() => greetingService.Greet(userConnection.CurrentUser.Name);
    }
}

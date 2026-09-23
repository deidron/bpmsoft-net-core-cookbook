namespace EP.EpNInjectContainerExample
{
    internal sealed class EpFeedNotificationChannel(IEpNotificationFormatter formatter) : IEpNotificationChannel
    {
        public string Name => "feed";

        public EpNotificationDelivery Send(string recipient, string text) =>
            new() { Channel = Name, Content = formatter.Format(recipient, text) };
    }
}

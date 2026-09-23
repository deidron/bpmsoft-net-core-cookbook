namespace EP.EpNInjectContainerExample
{
    internal sealed class EpEmailNotificationChannel(IEpNotificationFormatter formatter) : IEpNotificationChannel
    {
        public string Name => "email";

        public EpNotificationDelivery Send(string recipient, string text) =>
            new() { Channel = Name, Content = formatter.Format(recipient, text) };
    }
}

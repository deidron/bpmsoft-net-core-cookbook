namespace EP.EpNInjectContainerExample
{
    public interface IEpNotificationChannel
    {
        string Name { get; }

        EpNotificationDelivery Send(string recipient, string text);
    }
}

namespace EP.EpNInjectContainerExample
{
    using System.Collections.Generic;

    public interface IEpNotifier
    {
        IReadOnlyList<EpNotificationDelivery> Notify(string text);
    }
}

namespace EP.EpNInjectContainerExample
{
    using System.Collections.Generic;
    using System.Linq;
    using BPMSoft.Core;

    internal sealed class EpNotifier(UserConnection userConnection, IEnumerable<IEpNotificationChannel> channels)
        : IEpNotifier
    {
        public IReadOnlyList<EpNotificationDelivery> Notify(string text)
        {
            string recipient = userConnection.CurrentUser.Name;
            return channels.Select(channel => channel.Send(recipient, text)).ToList();
        }
    }
}

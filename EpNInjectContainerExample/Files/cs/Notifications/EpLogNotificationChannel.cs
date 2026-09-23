namespace EP.EpNInjectContainerExample
{
    using Common.Logging;

    internal sealed class EpLogNotificationChannel(IEpNotificationFormatter formatter) : IEpNotificationChannel
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpLogNotificationChannel));

        public string Name => "log";

        public EpNotificationDelivery Send(string recipient, string text)
        {
            string content = formatter.Format(recipient, text);
            s_log.Info(content);
            return new EpNotificationDelivery { Channel = Name, Content = content };
        }
    }
}

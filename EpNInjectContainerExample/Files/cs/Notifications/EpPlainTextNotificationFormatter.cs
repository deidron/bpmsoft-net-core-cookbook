namespace EP.EpNInjectContainerExample
{
    internal sealed class EpPlainTextNotificationFormatter : IEpNotificationFormatter
    {
        public string Format(string recipient, string text) => $"{recipient}: {text}";
    }
}

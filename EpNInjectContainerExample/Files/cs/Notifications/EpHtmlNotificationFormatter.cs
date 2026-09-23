namespace EP.EpNInjectContainerExample
{
    using System.Net;

    internal sealed class EpHtmlNotificationFormatter : IEpNotificationFormatter
    {
        public string Format(string recipient, string text) =>
            $"<p>Hello, <b>{WebUtility.HtmlEncode(recipient)}</b>!</p><p>{WebUtility.HtmlEncode(text)}</p>";
    }
}

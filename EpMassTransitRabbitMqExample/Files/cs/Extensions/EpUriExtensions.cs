namespace EP.EpMassTransitRabbitMqExample
{
    using System;

    internal static class EpUriExtensions
    {
        public static string ToLogString(this Uri address)
        {
            ArgumentNullException.ThrowIfNull(address);
            return address.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);
        }
    }
}

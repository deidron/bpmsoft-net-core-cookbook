namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Configuration;
    using BPMSoft.Core;

    internal static class EpAppConnectionExtensions
    {
        internal const string RabbitMqConnectionStringName = "epMassTransitRabbitMq";

        internal static Uri GetRabbitMqConnectionUri(this AppConnection appConnection) =>
            appConnection.GetConnectionStringUri(RabbitMqConnectionStringName);

        internal static Uri GetConnectionStringUri(this AppConnection appConnection, string name)
        {
            ArgumentNullException.ThrowIfNull(appConnection);
            return appConnection.AppSettings.RootConfiguration.GetConnectionStringUri(name);
        }

        internal static Uri GetConnectionStringUri(this System.Configuration.Configuration configuration, string name)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrEmpty(name);
            ConnectionStringSettings settings = configuration.ConnectionStrings.ConnectionStrings[name];
            return string.IsNullOrWhiteSpace(settings?.ConnectionString)
                ? throw new ConfigurationErrorsException($"Connection string '{name}' is not set in ConnectionStrings.config.")
                : new Uri(settings.ConnectionString);
        }
    }
}

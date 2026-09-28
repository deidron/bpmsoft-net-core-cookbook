namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Autofac;
    using MassTransit.Monitoring.Health;

    internal static class EpRabbitMqBus
    {
        public static Uri Address => new(Environment.GetEnvironmentVariable(EpRabbitMqFactAttribute.UriVariable));

        public static IContainer BuildContainer(Uri address) => EpMassTransitRabbitMqInitializer.CreateContainer(address);

        public static Uri WithPassword(Uri address, string password) => new UriBuilder(address) { Password = password }.Uri;

        public static Uri WithPort(Uri address, int port) => new UriBuilder(address) { Port = port }.Uri;

        public static bool WaitHealthy(ILifetimeScope root, TimeSpan timeout) =>
            WaitUntil(() => root.Resolve<IBusHealth>().CheckHealth().Status == BusHealthStatus.Healthy, timeout);

        public static bool WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!condition())
            {
                if (stopwatch.Elapsed > timeout)
                    return false;
                Thread.Sleep(100);
            }
            return true;
        }
    }
}

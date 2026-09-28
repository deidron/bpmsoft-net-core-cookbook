namespace EP.EpMassTransitRabbitMqExample
{
    using System.Collections.Generic;
    using System.Linq;
    using MassTransit.Monitoring.Health;

    internal sealed class EpBusHealthReporter(IBusHealth busHealth) : IEpBusHealthReporter
    {
        private readonly IBusHealth _busHealth = busHealth;

        public EpBusHealthResponse GetHealth()
        {
            HealthResult health = _busHealth.CheckHealth();
            return new EpBusHealthResponse
            {
                Healthy = health.Status == BusHealthStatus.Healthy,
                Status = health.Status.ToString(),
                Description = health.Description,
                Endpoints = GetEndpoints(health),
            };
        }

        private static List<string> GetEndpoints(HealthResult health) =>
            health.Data.TryGetValue("Endpoints", out object endpoints)
            && endpoints is IEnumerable<KeyValuePair<string, object>> items
            ? [.. items.Select(x => $"{x.Key}: {x.Value}")]
            : [];
    }
}

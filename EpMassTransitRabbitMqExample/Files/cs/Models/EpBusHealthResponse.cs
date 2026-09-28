namespace EP.EpMassTransitRabbitMqExample
{
    using System.Collections.Generic;

    public class EpBusHealthResponse
    {
        public bool Healthy { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<string> Endpoints { get; set; }
    }
}

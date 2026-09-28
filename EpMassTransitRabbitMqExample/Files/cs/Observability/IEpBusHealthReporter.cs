namespace EP.EpMassTransitRabbitMqExample
{
    internal interface IEpBusHealthReporter
    {
        EpBusHealthResponse GetHealth();
    }
}

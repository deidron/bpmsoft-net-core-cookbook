namespace EP.EpMassTransitRabbitMqExample.Api
{
    using BPMSoft.Core;

    internal interface IEpMassTransitRabbitMqInitializer
    {
        void Initialize(AppConnection appConnection);
        void Stop();
    }
}

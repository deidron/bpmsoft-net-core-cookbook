namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpBusLogObserver(EpReceiveEndpointLogObserver receiveEndpointObserver) : IBusObserver
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpBusLogObserver));

        private readonly EpReceiveEndpointLogObserver _receiveEndpointObserver = receiveEndpointObserver;

        public Task PostCreate(IBus bus)
        {
            _ = bus.ConnectReceiveEndpointObserver(_receiveEndpointObserver);
            return Task.CompletedTask;
        }

        public Task CreateFaulted(Exception exception)
        {
            s_log.Error("[MassTransit] Bus creation faulted.", exception);
            return Task.CompletedTask;
        }

        public Task PreStart(IBus bus) => Task.CompletedTask;

        public Task PostStart(IBus bus, Task<BusReady> busReady) => Task.CompletedTask;

        public Task StartFaulted(IBus bus, Exception exception) => Task.CompletedTask;

        public Task PreStop(IBus bus) => Task.CompletedTask;

        public Task PostStop(IBus bus) => Task.CompletedTask;

        public Task StopFaulted(IBus bus, Exception exception) => Task.CompletedTask;
    }
}

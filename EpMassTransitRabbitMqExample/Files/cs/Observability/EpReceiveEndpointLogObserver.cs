namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpReceiveEndpointLogObserver : IReceiveEndpointObserver
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpReceiveEndpointLogObserver));

        private readonly ConcurrentDictionary<Uri, long> _connectionLostAt = new();

        public Task Ready(ReceiveEndpointReady ready)
        {
            if (_connectionLostAt.TryRemove(ready.InputAddress, out long lostAt))
                s_log.Info($"[MassTransit] Connection to the broker restored after {Stopwatch.GetElapsedTime(lostAt):hh\\:mm\\:ss}: {ready.InputAddress}");
            else
                s_log.Info($"[MassTransit] Receive endpoint ready: {ready.InputAddress}");
            return Task.CompletedTask;
        }

        public Task Stopping(ReceiveEndpointStopping stopping) => Task.CompletedTask;

        public Task Completed(ReceiveEndpointCompleted completed)
        {
            _connectionLostAt[completed.InputAddress] = Stopwatch.GetTimestamp();
            s_log.Warn($"[MassTransit] Connection to the broker lost, receive endpoint stopped consuming: " +
                $"{completed.InputAddress} (delivered {completed.DeliveryCount}). MassTransit will reconnect.");
            return Task.CompletedTask;
        }

        public Task Faulted(ReceiveEndpointFaulted faulted)
        {
            s_log.Warn($"[MassTransit] Receive endpoint faulted: {faulted.InputAddress}", faulted.Exception);
            return Task.CompletedTask;
        }
    }
}

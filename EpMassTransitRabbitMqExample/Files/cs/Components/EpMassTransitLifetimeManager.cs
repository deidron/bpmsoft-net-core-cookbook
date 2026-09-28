namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Common.Logging;
    using MassTransit;

    internal sealed class EpMassTransitLifetimeManager : IStartable, IDisposable
    {
        private static readonly TimeSpan s_defaultStartGracePeriod = TimeSpan.FromSeconds(2);

        private static readonly TimeSpan s_defaultStopTimeout = TimeSpan.FromSeconds(30);

        private static readonly TimeSpan s_defaultNotReadyReportInterval = TimeSpan.FromSeconds(30);

        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpMassTransitLifetimeManager));

        private readonly IBusControl _busControl;

        private readonly TimeSpan _startGracePeriod;

        private readonly TimeSpan _stopTimeout;

        private readonly TimeSpan _notReadyReportInterval;

        private readonly CancellationTokenSource _startCancellation = new();

        private readonly object _sync = new();

        private Task<BusHandle> _startTask;

        private bool _disposed;

        public EpMassTransitLifetimeManager(IBusControl busControl)
            : this(busControl, s_defaultStartGracePeriod, s_defaultStopTimeout, s_defaultNotReadyReportInterval)
        {
        }

        internal EpMassTransitLifetimeManager(
            IBusControl busControl, TimeSpan startGracePeriod, TimeSpan stopTimeout, TimeSpan notReadyReportInterval)
        {
            _busControl = busControl;
            _startGracePeriod = startGracePeriod;
            _stopTimeout = stopTimeout;
            _notReadyReportInterval = notReadyReportInterval;
        }

        public void Start()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    s_log.Warn("[MassTransit] Start called after Dispose. Ignored. Restart the bus by rebuilding the container.");
                    return;
                }
                if (_startTask is not null)
                    return;
                _startTask = Task.Run(StartBusAsync);
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "An exception would abort Autofac's disposal loop and leave the remaining components undisposed.")]
        public void Dispose()
        {
            Task<BusHandle> startTask;
            lock (_sync)
            {
                if (_disposed)
                    return;
                _disposed = true;
                startTask = _startTask;
            }
            s_log.Debug("[MassTransit] Stopping bus control gracefully...");
            bool startFinished = false;
            try
            {
                startFinished = startTask is null || startTask.Wait(_startGracePeriod);
                if (!startFinished)
                {
                    _startCancellation.Cancel();
                    startFinished = startTask.Wait(_stopTimeout);
                }
                if (!startFinished)
                {
                    s_log.Warn($"[MassTransit] Bus start did not finish within {_stopTimeout} after cancellation. The bus is left as is.");
                    return;
                }
                BusHandle busHandle = startTask?.GetAwaiter().GetResult();
                if (busHandle is null)
                {
                    s_log.Debug("[MassTransit] Bus was not started. Nothing to stop.");
                    return;
                }
                if (!busHandle.StopAsync(_stopTimeout).Wait(_stopTimeout + _startGracePeriod))
                {
                    s_log.Warn($"[MassTransit] Bus stop did not finish within {_stopTimeout + _startGracePeriod}. The bus is left as is.");
                    return;
                }
                s_log.Debug("[MassTransit] Bus control stopped.");
            }
            catch (Exception ex)
            {
                s_log.Error("[MassTransit] Error during stop.", ex);
            }
            finally
            {
                if (startFinished)
                    _startCancellation.Dispose();
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Nobody observes the start task until shutdown. The log is the only place the error surfaces.")]
        private async Task<BusHandle> StartBusAsync()
        {
            try
            {
                s_log.Debug("[MassTransit] Starting bus control...");
                Task<BusHandle> start = _busControl.StartAsync(_startCancellation.Token);
                await ReportWhileNotReadyAsync(start).ConfigureAwait(false);
                BusHandle busHandle = await start.ConfigureAwait(false);
                s_log.Debug("[MassTransit] Bus control started successfully.");
                return busHandle;
            }
            catch (Exception ex)
            {
                if (_startCancellation.IsCancellationRequested)
                {
                    s_log.Debug("[MassTransit] Bus start cancelled by shutdown.", ex);
                    return null;
                }
                s_log.Error("[MassTransit] Failed to start bus.", ex);
                return null;
            }
        }

        private async Task ReportWhileNotReadyAsync(Task start)
        {
            TimeSpan waited = TimeSpan.Zero;
            while (true)
            {
                Task delay = Task.Delay(_notReadyReportInterval, _startCancellation.Token);
                if (await Task.WhenAny(start, delay).ConfigureAwait(false) == start || delay.IsCanceled)
                    return;
                waited += _notReadyReportInterval;
                s_log.Warn($"[MassTransit] Bus is not ready after {waited}: the broker is unreachable or does not accept the connection. MassTransit keeps retrying.");
            }
        }
    }

}

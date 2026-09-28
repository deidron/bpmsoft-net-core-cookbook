namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using NSubstitute;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    public sealed class EpMassTransitLifetimeManagerTests
    {
        private static readonly TimeSpan s_gracePeriod = TimeSpan.FromMilliseconds(200);

        private static readonly TimeSpan s_stopTimeout = TimeSpan.FromMilliseconds(500);

        private static readonly TimeSpan s_reportInterval = TimeSpan.FromMilliseconds(100);

        private static readonly Type s_source = typeof(EpMassTransitLifetimeManager);

        private readonly EpFakeBus _fake = new();

        public EpMassTransitLifetimeManagerTests() => EpLogCapture.Clear();

        [Fact]
        public void ConcurrentStartStartsBusOnce()
        {
            using EpMassTransitLifetimeManager manager = CreateManager();

            _ = Parallel.For(0, 8, _ => manager.Start());
            manager.Dispose();

            _ = _fake.Bus.Received(1).StartAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void StartAfterDisposeIsIgnoredWithWarning()
        {
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();
            manager.Dispose();

            manager.Start();

            _ = _fake.Bus.Received(1).StartAsync(Arg.Any<CancellationToken>());
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Warn, "Start called after Dispose"));
        }

        [Fact]
        public void ConcurrentDisposeStopsBusOnce()
        {
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();

            _ = Parallel.For(0, 8, _ => manager.Dispose());

            _ = _fake.Handle.Received(1).StopAsync(Arg.Any<CancellationToken>());
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Debug, "Bus control stopped"));
        }

        [Fact]
        public void DisposeWithoutStartDoesNotTouchBus()
        {
            using EpMassTransitLifetimeManager manager = CreateManager();

            manager.Dispose();

            _ = _fake.Bus.DidNotReceive().StartAsync(Arg.Any<CancellationToken>());
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Debug, "Nothing to stop"));
        }

        [Fact]
        public void DisposeStopsStartedBusWithoutCancellingStart()
        {
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();

            manager.Dispose();

            Assert.False(_fake.StartToken.IsCancellationRequested);
            _ = _fake.Handle.Received(1).StopAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void DisposeWaitsForStartWithinGracePeriod()
        {
            _fake.StartCompletesAfter(TimeSpan.FromMilliseconds(50));
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();

            manager.Dispose();

            Assert.False(_fake.StartToken.IsCancellationRequested);
            _ = _fake.Handle.Received(1).StopAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void DisposeCancelsStartAfterGracePeriod()
        {
            _fake.StartCompletesOnCancellation();
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();

            manager.Dispose();

            Assert.True(stopwatch.Elapsed >= s_gracePeriod);
            Assert.True(_fake.StartToken.IsCancellationRequested);
            _ = _fake.Handle.DidNotReceive().StopAsync(Arg.Any<CancellationToken>());
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Debug, "cancelled by shutdown"));
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Debug, "Nothing to stop"));
        }

        [Fact]
        public void DisposeAbandonsStartThatIgnoresCancellation()
        {
            _fake.StartNeverCompletes();
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();

            manager.Dispose();

            Assert.InRange(stopwatch.Elapsed, s_gracePeriod + s_stopTimeout, TimeSpan.FromSeconds(5));
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Warn, "Bus start did not finish"));
            Assert.Null(Record.Exception(() => _fake.StartToken.WaitHandle));
        }

        [Fact]
        public void DisposeAbandonsStopThatHangs()
        {
            _fake.StopNeverCompletes();
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();

            manager.Dispose();

            Assert.InRange(stopwatch.Elapsed, s_stopTimeout + s_gracePeriod, TimeSpan.FromSeconds(5));
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Warn, "Bus stop did not finish"));
        }

        [Fact]
        public void DisposeLogsErrorWhenStopFails()
        {
            _fake.StopFails(new InvalidOperationException("Stop failure."));
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();

            Exception exception = Record.Exception(manager.Dispose);

            Assert.Null(exception);
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Error, "Error during stop"));
        }

        [Fact]
        public void StartFailureIsLoggedAsError()
        {
            _fake.StartFails(new InvalidOperationException("Start failure."));
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();

            manager.Dispose();

            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Error, "Failed to start bus"));
            Assert.Equal(1, EpLogCapture.Count(s_source, LogLevel.Debug, "Nothing to stop"));
        }

        [Fact]
        public void WatchdogWarnsUntilBusIsReady()
        {
            _fake.StartCompletesAfter(TimeSpan.FromMilliseconds(350));
            using EpMassTransitLifetimeManager manager = CreateManager();

            manager.Start();
            Thread.Sleep(600);
            int warnings = NotReadyWarnings();
            Thread.Sleep(300);

            Assert.InRange(warnings, 2, 3);
            Assert.Equal(warnings, NotReadyWarnings());
        }

        [Fact]
        public void WatchdogStopsWhenStartIsCancelled()
        {
            _fake.StartCompletesOnCancellation();
            using EpMassTransitLifetimeManager manager = CreateManager();
            manager.Start();
            Thread.Sleep(250);

            manager.Dispose();
            int warnings = NotReadyWarnings();
            Thread.Sleep(300);

            Assert.True(warnings >= 1);
            Assert.Equal(warnings, NotReadyWarnings());
        }

        private static int NotReadyWarnings() => EpLogCapture.Count(s_source, LogLevel.Warn, "is not ready after");

        private EpMassTransitLifetimeManager CreateManager() =>
            new(_fake.Bus, s_gracePeriod, s_stopTimeout, s_reportInterval);
    }
}

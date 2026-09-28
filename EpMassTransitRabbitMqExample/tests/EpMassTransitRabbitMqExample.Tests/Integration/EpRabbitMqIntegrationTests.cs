namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Common.Logging;
    using MassTransit;
    using MassTransit.Monitoring.Health;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    [Trait("Category", "Integration")]
    public sealed class EpRabbitMqIntegrationTests : IDisposable
    {
        private static readonly TimeSpan s_readyTimeout = TimeSpan.FromSeconds(15);

        private static readonly Type s_manager = typeof(EpMassTransitLifetimeManager);

        private static readonly Type s_consumer = typeof(EpGreetingConsumer);

        private readonly string _run = Guid.NewGuid().ToString("N")[..8];

        public EpRabbitMqIntegrationTests()
        {
            EpLogCapture.Clear();
            EpMassTransitContainer.Shutdown();
        }

        public void Dispose() => EpMassTransitContainer.Shutdown();

        [EpRabbitMqFact]
        public void InitializerStartsWorkingBus()
        {
            EpMassTransitRabbitMqInitializer.Initialize(EpRabbitMqBus.Address);

            Assert.True(EpMassTransitContainer.TryGetRoot(out ILifetimeScope root));
            Assert.True(EpRabbitMqBus.WaitHealthy(root, s_readyTimeout), "Bus did not become healthy.");
            string message = Assert.Single(EpLogCapture.Events(typeof(EpMassTransitRabbitMqInitializer), LogLevel.Info)).RenderedMessage;
            Assert.DoesNotContain(EpRabbitMqBus.Address.UserInfo, message, StringComparison.Ordinal);
        }

        [EpRabbitMqFact]
        public void MixedInitializeAndStopRaceNeverHangs()
        {
            TimeSpan slowest = TimeSpan.Zero;
            for (int run = 0; run < 25; run++)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                _ = Parallel.For(0, 8, i =>
                {
                    if (i % 2 == 0)
                        _ = EpMassTransitContainer.TryInitialize(() => EpRabbitMqBus.BuildContainer(EpRabbitMqBus.Address));
                    else
                        EpMassTransitContainer.Shutdown();
                });
                slowest = stopwatch.Elapsed > slowest ? stopwatch.Elapsed : slowest;
            }
            EpMassTransitContainer.Shutdown();

            Assert.True(slowest < TimeSpan.FromSeconds(10), $"Slowest run took {slowest}.");
            Assert.Equal(0, EpLogCapture.Count(s_manager, LogLevel.Warn, "did not finish"));
            Assert.Empty(EpLogCapture.Events(s_manager, LogLevel.Error));
        }

        [EpRabbitMqFact]
        public void UnreachableBrokerStopCancelsAfterGracePeriod()
        {
            Uri address = EpRabbitMqBus.WithPort(EpRabbitMqBus.Address, 1);
            _ = EpMassTransitContainer.TryInitialize(() => EpRabbitMqBus.BuildContainer(address));
            Thread.Sleep(500);
            Stopwatch stopwatch = Stopwatch.StartNew();

            EpMassTransitContainer.Shutdown();

            Assert.InRange(stopwatch.Elapsed, TimeSpan.FromSeconds(1.9), TimeSpan.FromSeconds(10));
            Assert.Equal(1, EpLogCapture.Count(s_manager, LogLevel.Debug, "cancelled by shutdown"));
            Assert.Equal(1, EpLogCapture.Count(s_manager, LogLevel.Debug, "Nothing to stop"));
            Assert.Equal(0, EpLogCapture.Count(s_manager, LogLevel.Warn, "did not finish"));
        }

        [EpRabbitMqFact]
        public async Task ExternalStopLeavesBusDeadUntilContainerIsRebuilt()
        {
            ILifetimeScope root = StartBus();
            IBusHealth health = root.Resolve<IBusHealth>();

            await root.Resolve<IBusControl>().StopAsync().ConfigureAwait(true);
            bool stopped = EpRabbitMqBus.WaitUntil(() => health.CheckHealth().Status != BusHealthStatus.Healthy, s_readyTimeout);
            root.Resolve<IStartable>().Start();
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            BusHealthStatus afterStart = health.CheckHealth().Status;
            EpMassTransitContainer.Shutdown();

            Assert.True(stopped, "Bus stayed healthy after external stop.");
            Assert.NotEqual(BusHealthStatus.Healthy, afterStart);
            Assert.Empty(EpLogCapture.Events(s_manager, LogLevel.Error));
            _ = StartBus();
        }

        [EpRabbitMqFact]
        public async Task ExternalStopRacingPackageStopCompletes()
        {
            for (int i = 0; i < 2; i++)
            {
                IBusControl bus = StartBus().Resolve<IBusControl>();
                Task race = Task.WhenAll(Task.Run(() => bus.StopAsync()), Task.Run(EpMassTransitContainer.Shutdown));

                Task finished = await Task.WhenAny(race, Task.Delay(TimeSpan.FromSeconds(60))).ConfigureAwait(true);

                Assert.Same(race, finished);
                await race.ConfigureAwait(true);
            }
            Assert.Empty(EpLogCapture.Events(s_manager, LogLevel.Error));
        }

        [EpRabbitMqFact]
        public void BusStartsAndStopsGracefully()
        {
            ILifetimeScope root = StartBus();

            EpMassTransitContainer.Shutdown();

            Assert.Equal(1, EpLogCapture.Count(s_manager, LogLevel.Debug, "Bus control stopped"));
            Assert.Empty(EpLogCapture.Events(s_manager, LogLevel.Warn));
            GC.KeepAlive(root);
        }

        [EpRabbitMqFact]
        public void StopRightAfterStartNeverHangs()
        {
            TimeSpan slowest = TimeSpan.Zero;
            for (int i = 0; i < 20; i++)
            {
                _ = EpMassTransitContainer.TryInitialize(() => EpRabbitMqBus.BuildContainer(EpRabbitMqBus.Address));
                Stopwatch stopwatch = Stopwatch.StartNew();
                EpMassTransitContainer.Shutdown();
                slowest = stopwatch.Elapsed > slowest ? stopwatch.Elapsed : slowest;
            }

            Assert.True(slowest < TimeSpan.FromSeconds(10), $"Slowest stop took {slowest}.");
            Assert.Equal(0, EpLogCapture.Count(s_manager, LogLevel.Warn, "did not finish"));
            Assert.Equal(20, EpLogCapture.Count(s_manager, LogLevel.Debug, "Bus control stopped"));
        }

        [EpRabbitMqFact]
        public async Task ParallelPublishIsDelivered()
        {
            IEpGreetingPublisher publisher = StartBus().Resolve<IEpGreetingPublisher>();

            EpGreetingPublishResult[] results = await Task.WhenAll(Enumerable.Range(0, 100)
                .Select(i => Task.Run(() => publisher.PublishAsync($"load-{_run}-{i}")))).ConfigureAwait(true);

            Assert.All(results, result => Assert.Equal(EpGreetingPublishResult.Published, result));
            Assert.True(
                EpRabbitMqBus.WaitUntil(() => Delivered($"load-{_run}-") >= 100, s_readyTimeout),
                $"Delivered {Delivered($"load-{_run}-")} of 100.");
        }

        [EpRabbitMqFact]
        public async Task NotConfirmedGreetingMayStillBeDelivered()
        {
            EpGreetingPublisher publisher = new(StartBus().Resolve<IBus>(), TimeSpan.FromMilliseconds(1));
            string[] notConfirmed = [];

            for (int i = 0; i < 50; i++)
            {
                string name = $"race-{_run}-{i}";
                if (await publisher.PublishAsync(name).ConfigureAwait(true) == EpGreetingPublishResult.NotConfirmed)
                    notConfirmed = [.. notConfirmed, name];
            }

            Assert.NotEmpty(notConfirmed);
            Assert.True(
                EpRabbitMqBus.WaitUntil(() => notConfirmed.Any(name => Delivered($"{name}!") > 0), s_readyTimeout),
                $"None of {notConfirmed.Length} unconfirmed greetings was delivered.");
        }

        [EpRabbitMqFact]
        public void AuthenticationFailureFailsFastWithoutRetries()
        {
            Uri address = EpRabbitMqBus.WithPassword(EpRabbitMqBus.Address, "wrong-password");
            Stopwatch stopwatch = Stopwatch.StartNew();

            _ = EpMassTransitContainer.TryInitialize(() => EpRabbitMqBus.BuildContainer(address));
            bool failed = EpRabbitMqBus.WaitUntil(
                () => EpLogCapture.Count(s_manager, LogLevel.Error, "Failed to start bus") == 1, TimeSpan.FromSeconds(10));

            Assert.True(failed);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"Start failed after {stopwatch.Elapsed}.");
            Assert.True(EpMassTransitContainer.TryGetRoot(out ILifetimeScope root));
            Assert.Equal(BusHealthStatus.Unhealthy, root.Resolve<IBusHealth>().CheckHealth().Status);
        }

        [EpRabbitMqFact]
        public void RestartCyclesReleaseOldContainer()
        {
            StartBusWithoutKeepingRoot();
            WeakReference[] first = TrackCurrentContainer();

            for (int i = 0; i < 10; i++)
            {
                EpMassTransitContainer.Shutdown();
                StartBusWithoutKeepingRoot();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(first[0].IsAlive, "First container is still referenced.");
            Assert.False(first[1].IsAlive, "First bus is still referenced.");
        }

        private static ILifetimeScope StartBus()
        {
            Assert.True(EpMassTransitContainer.TryInitialize(() => EpRabbitMqBus.BuildContainer(EpRabbitMqBus.Address)));
            Assert.True(EpMassTransitContainer.TryGetRoot(out ILifetimeScope root));
            Assert.True(EpRabbitMqBus.WaitHealthy(root, s_readyTimeout), "Bus did not become healthy.");
            return root;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StartBusWithoutKeepingRoot() => _ = StartBus();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] TrackCurrentContainer()
        {
            _ = EpMassTransitContainer.TryGetRoot(out ILifetimeScope root);
            return [new WeakReference(root), new WeakReference(root.Resolve<IBusControl>())];
        }

        private static int Delivered(string fragment) => EpLogCapture.Count(s_consumer, LogLevel.Info, $"Hello, {fragment}");
    }
}

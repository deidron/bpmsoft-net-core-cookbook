namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Xunit;

    [Collection(EpStaticContainerGroup.Name)]
    public sealed class EpMassTransitContainerTests : IDisposable
    {
        public EpMassTransitContainerTests() => EpMassTransitContainer.Shutdown();

        public void Dispose() => EpMassTransitContainer.Shutdown();

        [Fact]
        public void TryGetRootReturnsFalseWhenNotInitialized()
        {
            bool initialized = EpMassTransitContainer.TryGetRoot(out ILifetimeScope root);

            Assert.False(initialized);
            Assert.Null(root);
        }

        [Fact]
        public void TryInitializeBuildsContainerOnce()
        {
            IContainer container = EmptyContainer();

            bool first = EpMassTransitContainer.TryInitialize(() => container);
            bool second = EpMassTransitContainer.TryInitialize(EmptyContainer);

            Assert.True(first);
            Assert.False(second);
            Assert.True(EpMassTransitContainer.TryGetRoot(out ILifetimeScope root));
            Assert.Same(container, root);
        }

        [Fact]
        public void ConcurrentTryInitializeBuildsSingleContainer()
        {
            int factoryCalls = 0;
            ConcurrentBag<bool> results = [];

            _ = Parallel.For(0, 8, _ => results.Add(EpMassTransitContainer.TryInitialize(() =>
            {
                _ = Interlocked.Increment(ref factoryCalls);
                Thread.Sleep(50);
                return EmptyContainer();
            })));

            Assert.Equal(1, factoryCalls);
            _ = Assert.Single(results, built => built);
            Assert.True(EpMassTransitContainer.TryGetRoot(out _));
        }

        [Fact]
        public void ConcurrentInitializeAndShutdownNeverLeakContainers()
        {
            int created = 0;
            int disposed = 0;

            for (int run = 0; run < 25; run++)
            {
                _ = Parallel.For(0, 8, i =>
                {
                    if (i % 2 == 0)
                    {
                        _ = EpMassTransitContainer.TryInitialize(() =>
                        {
                            _ = Interlocked.Increment(ref created);
                            return EpDisposeHook.BuildContainer(() => _ = Interlocked.Increment(ref disposed));
                        });
                    }
                    else
                    {
                        EpMassTransitContainer.Shutdown();
                    }
                });
            }
            EpMassTransitContainer.Shutdown();

            Assert.True(created > 0);
            Assert.Equal(created, disposed);
        }

        [Fact]
        public void TryInitializeRejectsNullFactory() =>
            Assert.Throws<ArgumentNullException>(() => EpMassTransitContainer.TryInitialize(null));

        [Fact]
        public void FactoryExceptionPropagatesAndLeavesStateClean()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => EpMassTransitContainer.TryInitialize(() => throw new InvalidOperationException("Factory failure.")));

            Assert.Equal("Factory failure.", exception.Message);
            AssertStateIsClean();
        }

        [Fact]
        public void FactoryReturningNullIsRejected()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => EpMassTransitContainer.TryInitialize(() => null));

            Assert.Contains("returned null", exception.Message, StringComparison.Ordinal);
            AssertStateIsClean();
        }

        [Fact]
        public void ReentrantTryInitializeWhileBuildingThrows()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => EpMassTransitContainer.TryInitialize(() =>
                {
                    _ = EpMassTransitContainer.TryInitialize(EmptyContainer);
                    return EmptyContainer();
                }));

            Assert.Contains("while Building", exception.Message, StringComparison.Ordinal);
            AssertStateIsClean();
        }

        [Fact]
        public void ReentrantShutdownWhileBuildingThrows()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => EpMassTransitContainer.TryInitialize(() =>
                {
                    EpMassTransitContainer.Shutdown();
                    return EmptyContainer();
                }));

            Assert.Contains("while it is being built", exception.Message, StringComparison.Ordinal);
            AssertStateIsClean();
        }

        [Fact]
        public void ShutdownWithoutContainerDoesNothing()
        {
            EpMassTransitContainer.Shutdown();

            AssertStateIsClean();
        }

        [Fact]
        public void ShutdownDisposesContainerAndClearsRoot()
        {
            bool disposed = false;
            _ = EpMassTransitContainer.TryInitialize(() => EpDisposeHook.BuildContainer(() => disposed = true));

            EpMassTransitContainer.Shutdown();

            Assert.True(disposed);
            AssertStateIsClean();
        }

        [Fact]
        public void TryGetRootReturnsFalseWhileDisposing()
        {
            bool? initializedDuringDispose = null;
            _ = EpMassTransitContainer.TryInitialize(
                () => EpDisposeHook.BuildContainer(() => initializedDuringDispose = EpMassTransitContainer.TryGetRoot(out _)));

            EpMassTransitContainer.Shutdown();

            Assert.False(initializedDuringDispose);
        }

        [Fact]
        public void ReentrantShutdownWhileDisposingIsIgnored()
        {
            bool disposed = false;
            _ = EpMassTransitContainer.TryInitialize(() => EpDisposeHook.BuildContainer(() =>
            {
                disposed = true;
                EpMassTransitContainer.Shutdown();
            }));

            EpMassTransitContainer.Shutdown();

            Assert.True(disposed);
            AssertStateIsClean();
        }

        [Fact]
        public void ReentrantTryInitializeWhileDisposingThrows()
        {
            Exception reentrantException = null;
            _ = EpMassTransitContainer.TryInitialize(() => EpDisposeHook.BuildContainer(
                () => reentrantException = Record.Exception(() => EpMassTransitContainer.TryInitialize(EmptyContainer))));

            EpMassTransitContainer.Shutdown();

            InvalidOperationException exception = Assert.IsType<InvalidOperationException>(reentrantException);
            Assert.Contains("while Disposing", exception.Message, StringComparison.Ordinal);
            AssertStateIsClean();
        }

        [Fact]
        public void ComponentDisposeExceptionPropagatesAndClearsContainer()
        {
            _ = EpMassTransitContainer.TryInitialize(
                () => EpDisposeHook.BuildContainer(() => throw new InvalidOperationException("Dispose failure.")));

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(EpMassTransitContainer.Shutdown);

            Assert.Equal("Dispose failure.", exception.Message);
            AssertStateIsClean();
        }

        private static IContainer EmptyContainer() => new ContainerBuilder().Build();

        private static void AssertStateIsClean()
        {
            Assert.False(EpMassTransitContainer.TryGetRoot(out _));
            Assert.True(EpMassTransitContainer.TryInitialize(EmptyContainer));
            EpMassTransitContainer.Shutdown();
        }
    }
}

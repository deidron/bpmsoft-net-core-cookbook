namespace EP.EpAutofacContainerExample
{
    using System;
    using System.Threading;
    using Autofac;
    using Common.Logging;

    internal static class EpAutofacContainer
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpAutofacContainer));

        private static readonly object s_sync = new();

        private static IContainer s_container;

        private static Operation s_operation;

        private enum Operation
        {
            None,
            Building,
            Disposing
        }

        internal static ILifetimeScope Root =>
            Volatile.Read(in s_container)
            ?? throw new InvalidOperationException("Autofac Container is not initialized.");

        internal static bool TryInitialize(Func<IContainer> createContainer)
        {
            ArgumentNullException.ThrowIfNull(createContainer);
            lock (s_sync)
            {
                if (s_operation != Operation.None)
                    throw new InvalidOperationException($"Autofac Container must not be initialized while {s_operation}.");
                if (s_container is not null)
                    return false;
                s_operation = Operation.Building;
                try
                {
                    IContainer container = createContainer()
                        ?? throw new InvalidOperationException("Autofac Container factory returned null.");
                    Volatile.Write(ref s_container, container);
                }
                finally
                {
                    s_operation = Operation.None;
                }
            }
            s_log.Debug("Autofac Container built.");
            return true;
        }

        internal static void Shutdown()
        {
            lock (s_sync)
            {
                if (s_operation == Operation.Disposing)
                    return;
                if (s_operation == Operation.Building)
                    throw new InvalidOperationException("Autofac Container must not be shut down while it is being built.");
                IContainer container = s_container;
                if (container is null)
                {
                    s_log.Debug("Autofac Container is not set. Nothing to dispose.");
                    return;
                }
                Volatile.Write(ref s_container, null);
                s_operation = Operation.Disposing;
                try
                {
                    container.Dispose();
                }
                finally
                {
                    s_operation = Operation.None;
                }
            }
            s_log.Debug("Autofac Container disposed.");
        }
    }
}

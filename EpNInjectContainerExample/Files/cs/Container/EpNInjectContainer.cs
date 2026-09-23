namespace EP.EpNInjectContainerExample
{
    using System;
    using System.Threading;
    using Common.Logging;
    using Ninject;

    internal static class EpNInjectContainer
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof(EpNInjectContainer));

        private static readonly object s_sync = new();

        private static IKernel s_kernel;

        private static Operation s_operation;

        private enum Operation
        {
            None,
            Building,
            Disposing
        }

        internal static IKernel Root =>
            Volatile.Read(in s_kernel)
            ?? throw new InvalidOperationException("Ninject Kernel is not initialized.");

        internal static bool TryInitialize(Func<IKernel> createContainer)
        {
            ArgumentNullException.ThrowIfNull(createContainer);
            lock (s_sync)
            {
                if (s_operation != Operation.None)
                    throw new InvalidOperationException($"Ninject Kernel must not be initialized while {s_operation}.");
                if (s_kernel is not null)
                    return false;
                s_operation = Operation.Building;
                try
                {
                    IKernel container = createContainer()
                        ?? throw new InvalidOperationException("Ninject Kernel factory returned null.");
                    Volatile.Write(ref s_kernel, container);
                }
                finally
                {
                    s_operation = Operation.None;
                }
            }
            s_log.Debug("Ninject Kernel built.");
            return true;
        }

        internal static void Shutdown()
        {
            lock (s_sync)
            {
                if (s_operation == Operation.Disposing)
                    return;
                if (s_operation == Operation.Building)
                    throw new InvalidOperationException("Ninject Kernel must not be shut down while it is being built.");
                IKernel kernel = s_kernel;
                if (kernel is null)
                {
                    s_log.Debug("Ninject Kernel is not set; nothing to dispose.");
                    return;
                }
                Volatile.Write(ref s_kernel, null);
                s_operation = Operation.Disposing;
                try
                {
                    kernel.Dispose();
                }
                finally
                {
                    s_operation = Operation.None;
                }
            }
            s_log.Debug("Ninject Kernel disposed.");
        }
    }
}

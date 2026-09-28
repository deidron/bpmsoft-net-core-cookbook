namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Logging;
    using Common.Logging.Simple;

    internal static class EpLogCapture
    {
        private static readonly CapturingLoggerFactoryAdapter s_adapter = Install();

        public static void Clear() => s_adapter.Clear();

        public static IReadOnlyList<CapturingLoggerEvent> Events(Type source, LogLevel level)
        {
            lock (s_adapter.LoggerEvents)
            {
                return [.. s_adapter.LoggerEvents.Where(e => e.Source.Name == source.FullName && e.Level == level)];
            }
        }

        public static int Count(Type source, LogLevel level, string fragment) =>
            Events(source, level).Count(e => e.RenderedMessage.Contains(fragment, StringComparison.Ordinal));

        private static CapturingLoggerFactoryAdapter Install()
        {
            CapturingLoggerFactoryAdapter adapter = new();
            LogManager.Adapter = adapter;
            return adapter;
        }
    }
}

namespace EP.EpMassTransitRabbitMqExample
{
    using System;
    using Common.Logging;
    using Microsoft.Extensions.Logging;
    using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

    internal sealed class EpMassTransitLoggerFactory(string categoryPrefix) : ILoggerFactory
    {
        private readonly string _categoryPrefix = categoryPrefix;

        public ILogger CreateLogger(string categoryName) =>
            new EpCommonLoggingLogger(LogManager.GetLogger($"{_categoryPrefix}.{categoryName}"));

        public void AddProvider(ILoggerProvider provider) =>
            throw new NotSupportedException($"{nameof(EpMassTransitLoggerFactory)} does not support logger providers.");

        public void Dispose()
        { }

        private sealed class EpCommonLoggingLogger(ILog log) : ILogger
        {
            private readonly ILog _log = log;

            IDisposable ILogger.BeginScope<TState>(TState state) => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel switch
            {
                MelLogLevel.Trace => _log.IsTraceEnabled,
                MelLogLevel.Debug => _log.IsDebugEnabled,
                MelLogLevel.Information => _log.IsInfoEnabled,
                MelLogLevel.Warning => _log.IsWarnEnabled,
                MelLogLevel.Error => _log.IsErrorEnabled,
                MelLogLevel.Critical => _log.IsFatalEnabled,
                MelLogLevel.None => false,
                _ => false,
            };

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;
                string message = formatter(state, exception);
                switch (logLevel)
                {
                    case MelLogLevel.Trace:
                        _log.Trace(message, exception);
                        break;
                    case MelLogLevel.Debug:
                        _log.Debug(message, exception);
                        break;
                    case MelLogLevel.Information:
                        _log.Info(message, exception);
                        break;
                    case MelLogLevel.Warning:
                        _log.Warn(message, exception);
                        break;
                    case MelLogLevel.Error:
                        _log.Error(message, exception);
                        break;
                    case MelLogLevel.Critical:
                        _log.Fatal(message, exception);
                        break;
                    case MelLogLevel.None:
                    default:
                        break;
                }
            }
        }
    }
}

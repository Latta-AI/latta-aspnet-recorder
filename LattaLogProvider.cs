using Microsoft.Extensions.Logging;

namespace LattaASPNet
{
    public class LattaLogProvider : ILoggerProvider
    {
        private LattaLogger logger;

        public LattaLogProvider()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            logger = new LattaLogger();
            return logger;
        }

        public void Dispose()
        {
            logger = null;
        }
    }

    public class LattaLogger : ILogger
    {
        private static List<LattaLog> logs = new List<LattaLog>();

        public LattaLogger()
        {
        }

        public static IEnumerable<LattaLog> getLogs()
        {
            var currentLogs = new List<LattaLog>(logs);
            logs.Clear();

            return currentLogs.ToArray();
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Debug;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
                return;

            logs.Add(new LattaLog(message, "DEBUG"));
        }
    }
}

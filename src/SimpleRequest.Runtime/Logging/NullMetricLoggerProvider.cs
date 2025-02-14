namespace SimpleRequest.Runtime.Logging;

public class NullMetricLoggerProvider : IMetricLoggerProvider {
    private static readonly IMetricLogger _logger = new NullMetricsLogger();

    public IMetricLogger CreateLogger(string loggerName) {
        return _logger;
    }
}
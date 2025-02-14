namespace SimpleRequest.Runtime.Logging;

public interface IMetricLoggerProvider {
    IMetricLogger CreateLogger(string loggerName);
}
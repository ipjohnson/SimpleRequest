using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Logging;

[SingletonService(Using = RegistrationType.Try)]
public class NullMetricLoggerProvider : IMetricLoggerProvider {
    private static readonly IMetricLogger _logger = new NullMetricsLogger();

    public IMetricLogger CreateLogger(string? loggerName) {
        return _logger;
    }
}
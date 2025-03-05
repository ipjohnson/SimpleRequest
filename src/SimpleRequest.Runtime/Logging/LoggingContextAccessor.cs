using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Logging;

[SingletonService(Realm = typeof(EnhancedLoggingSupport))]
public class LoggingContextAccessor : ILoggingContextAccessor {
    private readonly AsyncLocal<List<RequestLoggingData>> _loggingContexts = new();

    public IReadOnlyList<RequestLoggingData> LogData => List;

    public void Clear() {
        List.Clear();
    }

    public void Add(RequestLoggingData data) {
        List.Add(data);
    }

    public void AddRange(IEnumerable<RequestLoggingData> data) {
        List.AddRange(data);
    }

    public void SetList(List<RequestLoggingData> data) {
        _loggingContexts.Value = data;
    }

    private List<RequestLoggingData> List => _loggingContexts.Value ??= [];
}
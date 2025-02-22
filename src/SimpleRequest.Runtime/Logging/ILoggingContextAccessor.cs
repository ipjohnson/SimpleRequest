namespace SimpleRequest.Runtime.Logging;

public interface ILoggingContextAccessor {
    IReadOnlyList<RequestLoggingData> LogData { get; }

    void Clear();
    
    void Add(RequestLoggingData data);
    
    void AddRange(IEnumerable<RequestLoggingData> data);
    
    void SetList(List<RequestLoggingData> data);
}
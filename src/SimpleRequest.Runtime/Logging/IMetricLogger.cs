using SimpleRequest.Runtime.Diagnostics;

namespace SimpleRequest.Runtime.Logging;

public interface IMetricLogger : IDisposable {
    Task Flush();

    void Record(IMetricDefinition metric, MachineTimestamp timestamp) {
        Record(metric, timestamp.GetElapsedMilliseconds());
    }
    
    void Record(IMetricDefinition metric, double value);

    void Tag(string tagName, object tagValue);

    void Data(string dataName, object dataValue);
}
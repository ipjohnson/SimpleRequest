using SimpleRequest.Runtime.Diagnostics;

namespace SimpleRequest.Runtime.Logging;

public interface IMetricLogger : IAsyncDisposable {
    Task Flush();
    
    void StartTimer(IMetricDefinition metricDefinition);
    
    void StopTimer(IMetricDefinition metricDefinition);
    
    void Increment(IMetricDefinition metricDefinition, double amount = 1.0);

    void Record(IMetricDefinition metric, MachineTimestamp timestamp) {
        Record(metric, timestamp.GetElapsedMilliseconds());
    }
    
    /// <summary>
    /// Record
    /// </summary>
    /// <param name="metric"></param>
    /// <param name="value"></param>
    void Record(IMetricDefinition metric, double value);

    /// <summary>
    /// Add tags to metrics logger
    /// </summary>
    /// <param name="tagName"></param>
    /// <param name="tagValue"></param>
    void Tag(string tagName, object tagValue);

    /// <summary>
    /// Clear all tags matching predicate, if no predicate provided
    /// all tags will be removed.
    /// </summary>
    /// <param name="predicate"></param>
    void ClearTags(Func<KeyValuePair<string, object>, bool>? predicate = null);
    
    /// <summary>
    /// Set data for metrics logger
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="dataValue"></param>
    void Data(string dataName, object dataValue);

    /// <summary>
    /// Clear all data matching the predicate, if no predicate is provided
    /// remove all data
    /// </summary>
    void ClearData(Func<KeyValuePair<string, object>, bool>? predicate = null);

    /// <summary>
    /// Clone dimension and data into new metrics logger
    /// </summary>
    /// <returns></returns>
    IMetricLogger Clone();
}
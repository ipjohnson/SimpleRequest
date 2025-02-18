﻿using SimpleRequest.Runtime.Diagnostics;

namespace SimpleRequest.Runtime.Logging;

public interface IMetricLogger : IAsyncDisposable {
    Task Flush();
    
    void StartTimer(IMetricDefinition metricDefinition);
    
    void StopTimer(IMetricDefinition metricDefinition);
    
    void Increment(IMetricDefinition metricDefinition, double amount = 1.0);

    void Record(IMetricDefinition metric, MachineTimestamp timestamp) {
        Record(metric, timestamp.GetElapsedMilliseconds());
    }
    
    void Record(IMetricDefinition metric, double value);

    void Tag(string tagName, object tagValue);

    void Data(string dataName, object dataValue);

    IMetricLogger Clone();
}
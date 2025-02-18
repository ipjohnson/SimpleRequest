namespace SimpleRequest.Runtime.Logging;

public class NullMetricsLogger : IMetricLogger {

    public Task Flush() {
        return Task.CompletedTask;
    }

    public void StartTimer(IMetricDefinition metricDefinition) { }

    public void StopTimer(IMetricDefinition metricDefinition) { }

    public void Increment(IMetricDefinition metricDefinition, double amount = 1) { }

    public void Record(IMetricDefinition metric, double value) { }

    public void Tag(string tagName, object tagValue) { }

    public void ClearTags(Func<KeyValuePair<string, object>, bool>? predicate = null) { }

    public void Data(string dataName, object dataValue) { }

    public void ClearData(Func<KeyValuePair<string, object>, bool>? predicate = null) { }

    public IMetricLogger Clone() {
        return this;
    }

    public ValueTask DisposeAsync() {
        return ValueTask.CompletedTask;
    }
}
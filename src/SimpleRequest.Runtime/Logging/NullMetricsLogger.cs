namespace SimpleRequest.Runtime.Logging;

public class NullMetricsLogger : IMetricLogger {
    public void Dispose() { }

    public Task Flush() {
        return Task.CompletedTask;
    }

    public void Increment(IMetricDefinition metricDefinition, double amount = 1) { }

    public void Record(IMetricDefinition metric, double value) { }

    public void Tag(string tagName, object tagValue) { }

    public void Data(string dataName, object dataValue) { }

    public IMetricLogger Clone() {
        return this;
    }
}
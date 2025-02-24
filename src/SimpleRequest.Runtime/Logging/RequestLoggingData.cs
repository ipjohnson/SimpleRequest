namespace SimpleRequest.Runtime.Logging;

[Flags]
public enum LoggingDataFeature {
    None = 0,
    LogData = 1,
    MetricData = 2,
    MetricValue = 4,
    MetricTag = 8
}

public record RequestLoggingData(
    string Key,
    object Value,
    LoggingDataFeature Feature = LoggingDataFeature.LogData | LoggingDataFeature.MetricData,
    MetricDefinition? MetricDefinition = null) {
    public KeyValuePair<string,object> AsKeyValuePair() => new (Key, Value);
};
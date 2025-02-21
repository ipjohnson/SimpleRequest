namespace SimpleRequest.Runtime.Logging;

[Flags]
public enum LoggingDataFeature {
    None = 0,
    LogData = 1,
    MetricData = 2,
    MetricTag = 4
}

public record RequestLoggingData(
    string Key,
    object Value,
    LoggingDataFeature Feature = LoggingDataFeature.LogData | LoggingDataFeature.MetricData) {
    public KeyValuePair<string,object> AsKeyValuePair() => new (Key, Value);
};
namespace SimpleRequest.Runtime.Logging;

public record MetricUnits(string Name) {

    public static readonly MetricUnits Milliseconds = new("Milliseconds");

    public static readonly MetricUnits Seconds = new("Seconds");

    public static readonly MetricUnits Count = new("Count");
}
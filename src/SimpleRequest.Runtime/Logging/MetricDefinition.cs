namespace SimpleRequest.Runtime.Logging;

public interface IMetricDefinition {
    string Name { get; }

    MetricUnits Units { get; }
}

public class MetricDefinition : IMetricDefinition {
    public MetricDefinition(string name, MetricUnits units) {
        Name = name;
        Units = units;
    }

    public string Name { get; }

    public MetricUnits Units { get; }
}
namespace SimpleRequest.Runtime.Logging;

public interface IMetricDefinition {
    string Name { get; }

    MetricUnits Units { get; }
}

public record MetricDefinition(string Name, MetricUnits Units) : IMetricDefinition;
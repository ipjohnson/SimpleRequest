namespace SimpleRequest.Runtime.Attributes;

/// <summary>
/// Attribute DependencyModules with this to control
/// the order modules are queried.
/// </summary>
/// <param name="order"></param>
public class RoutingOrderAttribute(int order) : Attribute {
    public int Order { get; } = order;
}
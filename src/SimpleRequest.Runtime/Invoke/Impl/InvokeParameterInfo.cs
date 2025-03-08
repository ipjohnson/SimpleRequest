namespace SimpleRequest.Runtime.Invoke.Impl;

public record InvokeParameterInfo(
    string Name,
    int Index,
    Type Type,
    ParameterBindType BindingType,
    string? DefaultValue,
    bool IsRequired,
    string BindingName,
    IReadOnlyList<Attribute> Attributes) : IInvokeParameterInfo;
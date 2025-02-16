namespace SimpleRequest.Runtime.Invoke.Impl;

public record InvokeParameterInfo(
    string Name,
    int Index,
    Type Type,
    ParameterBindType BindingType,
    IReadOnlyList<Attribute> Attributes) : IInvokeParameterInfo;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;

public record OperationInfo(
    PathDefinition Path,
    string Method, 
    Type HandlerType, 
    int? SuccessStatus,
    int? FailureStatus,
    int? NullResponseStatus, 
    IReadOnlyList<Attribute> Attributes,
    IReadOnlyList<IOperationParameterInfo> Parameters,
    Type ResponseType) : IOperationInfo;

public record OperationParameterInfo<T>(
    string Name,
    int Index,
    string BindingName,
    ParameterBindType BindingType,
    object? DefaultValue,
    bool IsRequired,
    IReadOnlyList<Attribute> Attributes) : IOperationParameterInfo {

    public Type Type {
        get;
    } = typeof(T);

    public void InvokeGenericCapture(IGenericParameterCapture receiver) {
        receiver.Capture<T>(this);
    }
}
    
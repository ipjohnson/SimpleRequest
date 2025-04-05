namespace SimpleRequest.Runtime.Invoke.Impl;

public record InvokeParameterInfo<T>(
    string Name,
    int Index,
    ParameterBindType BindingType,
    string? DefaultValue,
    bool IsRequired,
    string BindingName,
    IReadOnlyList<Attribute> Attributes) : IInvokeParameterInfo {

    public Type Type { get; } = typeof(T);
    
    public void InvokeGenericCapture(IGenericParameterCapture receiver) {
        receiver.Capture<T>(this);
    }
}


public interface IGenericParameterInvoker {
    void Invoke(IGenericParameterCapture receiver);
}



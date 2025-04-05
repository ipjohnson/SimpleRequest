namespace SimpleRequest.Runtime.Invoke;

public enum ParameterBindType {
    Path,
    QueryString,
    Header,
    Cookie,
    Body,
    ServiceProvider,
    FromServiceProvider,
    RequestContext,
    RequestData,
    ResponseData,
    CustomAttribute,
}

public interface IInvokeParameterInfo {
    string Name { get; }

    int Index { get; }

    Type Type { get; }
    
    string BindingName { get; }
    
    ParameterBindType BindingType { get; }
    
    string? DefaultValue { get; }
    
    bool IsRequired { get; }
    
    IReadOnlyList<Attribute> Attributes { get; }
    
    void InvokeGenericCapture(IGenericParameterCapture receiver);
}

public interface IGenericParameterCapture {
    void Capture<T>(IInvokeParameterInfo parameterInfo);
}
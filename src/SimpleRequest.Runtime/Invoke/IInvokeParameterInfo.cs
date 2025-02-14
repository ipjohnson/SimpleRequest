namespace SimpleRequest.Runtime.Invoke;

public enum ParameterBindType {
    Path,
    QueryString,
    Header,
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
    
    ParameterBindType BindingType { get; }
}
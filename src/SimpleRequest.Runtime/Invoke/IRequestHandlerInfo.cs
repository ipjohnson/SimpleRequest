namespace SimpleRequest.Runtime.Invoke;

public delegate ValueTask BindParametersDelegate(IRequestContext context);

public delegate IInvokeParameters ParametersCreationDelegate(IRequestContext context);

public delegate Task InvokeHandlerDelegate(IRequestContext context);

public interface IRequestHandlerInfoMethods {
    string InvokeMethod { get; }
    
    InvokeHandlerDelegate InvokeHandler { get; }
    
    ParametersCreationDelegate CreateParameters { get; }
    
    BindParametersDelegate BindParameters { get; }
    
    IReadOnlyList<IInvokeParameterInfo> Parameters { get; }
    
    Type ResponseType { get; }
}

public interface IRequestHandlerInfo {
    string Path { get; }

    string Method { get; }

    Type HandlerType { get; }

    int? SuccessStatus { get; }
    
    int? FailureStatus  { get; }

    int? NullResponseStatus  { get; }
    
    IReadOnlyList<Attribute> Attributes { get; }

    IRequestHandlerInfoMethods InvokeInfo { get; }
}
namespace SimpleRequest.Runtime.Invoke.Impl;

public record RequestHandlerInfo(
    string Path,
    string Method,
    Type HandlerType,
    int? SuccessStatus,
    int? FailureStatus,
    int? NullResponseStatus,
    IReadOnlyList<Attribute> Attributes,
    IRequestHandlerInfoMethods InvokeInfo
    ) : IRequestHandlerInfo;
    
public record RequestHandlerInfoMethods(
    string InvokeMethod,
    InvokeHandlerDelegate InvokeHandler,
    ParametersCreationDelegate CreateParameters,
    BindParametersDelegate BindParameters,
    IReadOnlyList<IInvokeParameterInfo> Parameters
    ) : IRequestHandlerInfoMethods;
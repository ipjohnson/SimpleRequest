using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.Runtime.StaticContentImpl;

public class StaticHandlerInfo : IRequestHandlerInfo {
    public StaticHandlerInfo(string path,
        string finalPath,
        IStaticContentResponseService staticContentResponseService,
        StaticContentProviderConfiguration contentProviderConfiguration) {
        Path = path;
        InvokeInfo = new StaticHandlerInfoMethods(finalPath, staticContentResponseService, contentProviderConfiguration);
    }
    
    public string Path { get; } 

    public string Method => "GET";

    public Type HandlerType => typeof(StaticHandlerInfoMethods);

    public int? SuccessStatus => 200;
    

    public int? FailureStatus => 500;

    public int? NullResponseStatus => 404;

    public IReadOnlyList<Attribute> Attributes => Array.Empty<Attribute>();

    public IRequestHandlerInfoMethods InvokeInfo { get; }
}

public class StaticHandlerInfoMethods : IRequestHandlerInfoMethods {

    public StaticHandlerInfoMethods(string finalPath, IStaticContentResponseService staticContentResponseService,
        StaticContentProviderConfiguration contentProviderConfiguration) {
        var storage = new StaticContentStorage(finalPath);
        
        InvokeHandler = 
            context => staticContentResponseService.Invoke(context, contentProviderConfiguration, storage);
    }
    
    public string InvokeMethod => "Invoke";

    public InvokeHandlerDelegate InvokeHandler {
        get;
    }

    public ParametersCreationDelegate CreateParameters => EmptyInvokeParameters.CreationDelegate;

    public BindParametersDelegate BindParameters => EmptyInvokeParameters.BindDelegate;

    public IReadOnlyList<IInvokeParameterInfo> Parameters => Array.Empty<IInvokeParameterInfo>();

    public Type ResponseType => typeof(void);
}
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.JsonRpc.Models;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;

namespace SimpleRequest.JsonRpc.Impl;



public class JsonRpcRoutingHandlerInfo : IRequestHandlerInfo {
    private readonly string _tag;
    private readonly bool _processInParallel;
    private IJsonRpcRequestProcessor? _processor;
    
    public JsonRpcRoutingHandlerInfo(string path, string tag, bool processInParallel) {
        _tag = tag;
        _processInParallel = processInParallel;
        Path = path;
        InvokeInfo = new RequestHandlerInfoMethods(
            "Invoke",
            InvokeHandler,
            EmptyInvokeParameters.CreationDelegate,
            EmptyInvokeParameters.BindDelegate,
            ArraySegment<IInvokeParameterInfo>.Empty,
            typeof(JsonRpcResponseModel)
        );
    }

    private Task InvokeHandler(IRequestContext context) {
        _processor ??= context.ServiceProvider.GetRequiredService<IJsonRpcRequestProcessor>();
        
        return _processor.HandleRequest(context, _tag, _processInParallel);
    }

    public string Path {
        get;
    }

    public string Method => "POST";

    public Type HandlerType => GetType();

    public int? SuccessStatus => null;

    public int? FailureStatus => null;

    public int? NullResponseStatus => null;

    public IReadOnlyList<Attribute> Attributes => Array.Empty<Attribute>();

    public IRequestHandlerInfoMethods InvokeInfo {
        get;
    }
}
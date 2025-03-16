namespace SimpleRequest.Runtime.Invoke.Impl;

public class RequestHandler : IRequestHandler {
    private readonly IRequestHandlerInfo _requestHandlerInfo;
    private readonly IReadOnlyList<FilterProvider> _filters;

    public RequestHandler(IRequestHandlerInfo requestHandlerInfo, IReadOnlyList<FilterProvider> filters) {
        _filters = filters;
        _requestHandlerInfo = requestHandlerInfo;
    }

    public IRequestHandlerInfo RequestHandlerInfo => _requestHandlerInfo;

    public Task Invoke(IRequestContext context) {
        context.RequestHandlerInfo = _requestHandlerInfo;
        
        var chain = new RequestChain(_filters, context);
        
        return chain.Next();
    }

    public bool CanHandle(IRequestContext context) {
        foreach (var attribute in _requestHandlerInfo.Attributes) {
            if (attribute is IExtendedRouteMatch match && !match.IsMatch(context)) {
                return false;
            }
        }
        
        return true;
    }
}
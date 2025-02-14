using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Invoke.Impl;

[SingletonService]
public class RequestInvocationEngine : IRequestInvocationEngine {
    private readonly IReadOnlyList<IRequestHandlerProvider> _providers;
    private readonly INotFoundHandler _notFoundHandler;

    public RequestInvocationEngine(
        IEnumerable<IRequestHandlerProvider> providers, 
        INotFoundHandler notFoundHandler) {
        _notFoundHandler = notFoundHandler;
        _providers = providers.Reverse().ToArray();
    }

    public Task Invoke(IRequestContext context) {
        for (var i = 0; i < _providers.Count; i++) {
            var handler = _providers[i].GetRequestHandler(context);

            if (handler != null) {
                context.RequestHandlerInfo = handler.RequestHandlerInfo;
                
                return handler.Invoke(context);
            }
        }
        
        return _notFoundHandler.Handle(context);
    }
}
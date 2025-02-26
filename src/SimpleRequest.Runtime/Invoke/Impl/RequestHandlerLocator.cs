using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Invoke.Impl;

[SingletonService]
public class RequestHandlerLocator : IRequestHandlerLocator {
    private readonly IRequestHandlerProvider[] _providers;

    public RequestHandlerLocator(
        IEnumerable<IRequestHandlerProvider> providers) {
        _providers = 
            providers.Reverse().OrderBy(x => x.Order).ToArray();
    }

    public IRequestHandler? GetHandler(IRequestContext context) {
        for (var i = 0; i < _providers.Length; i++) {
            var handler = _providers[i].GetRequestHandler(context);

            if (handler != null) {
                return handler;
            }
        }

        return null;
    }
}
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Filters;

public interface IIoFilterProvider {
    RequestFilterInfo ProviderFilter(IRequestHandlerInfo requestHandler);
}

[SingletonService]
public class IoFilterProvider : IIoFilterProvider {
    private readonly RequestFilterInfo _ioFilter;
    
    public IoFilterProvider(IRequestContextSerializer requestContextSerializer) {
        var filter = new IoRequestFilter(requestContextSerializer);
        _ioFilter = new RequestFilterInfo(_ => filter,
            (int)RequestFilterOrder.BindParameters);
    }

    public RequestFilterInfo ProviderFilter(IRequestHandlerInfo requestHandler) {
        return _ioFilter;
    }
}
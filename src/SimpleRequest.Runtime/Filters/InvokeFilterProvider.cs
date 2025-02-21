using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Filters;

public interface IInvokeFilterProvider {
    RequestFilterInfo ProvideFilter(IServiceProvider services, IRequestHandlerInfo info);
}

[SingletonService]
public class InvokeFilterProvider : IInvokeFilterProvider {
    private readonly RequestFilterInfo _invokeFilterInfo;
    
    public InvokeFilterProvider(IRequestContextSerializer serializer) {
        var filter = new InvokeFilter();
        _invokeFilterInfo = new RequestFilterInfo(_ => filter, (int)RequestFilterOrder.Last);
    }
    
    public RequestFilterInfo ProvideFilter(IServiceProvider services, IRequestHandlerInfo info) {
        return _invokeFilterInfo;
    }
}
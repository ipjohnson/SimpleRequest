using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Filters;

public interface IIoFilterProvider {
    RequestFilterInfo ProviderFilter(IServiceProvider services, IRequestHandlerInfo requestHandler);
}

[SingletonService]
public class IoFilterProvider : IIoFilterProvider {
    private readonly RequestFilterInfo _ioFilter;
    
    public IoFilterProvider(
        IRequestCompressionService requestCompressionService,
        IRequestContextSerializer requestContextSerializer,
        IRequestLoggingDataProviderService requestLoggingDataProviderService,
        ILoggingContextAccessor? loggingContextAccessor = null) {
        var filter = new IoRequestFilter(
            requestCompressionService,
            requestContextSerializer, 
            requestLoggingDataProviderService,
            loggingContextAccessor);
        
        _ioFilter = new RequestFilterInfo(_ => filter, RequestFilterOrder.BindParameters);
    }

    public RequestFilterInfo ProviderFilter(IServiceProvider services, IRequestHandlerInfo requestHandler) {
        return _ioFilter;
    }
}
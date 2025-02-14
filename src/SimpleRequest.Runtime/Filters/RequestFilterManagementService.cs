using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Filters;

public interface IRequestFilterManagementService {
    IReadOnlyList<FilterProvider> GetFilters(
        IRequestHandlerInfo requestHandlerInfo,
        params object[] providers
        );
}

[SingletonService]
public class RequestFilterManagementService : IRequestFilterManagementService {
    private readonly IGlobalFilterService _globalFilterService;
    private readonly IInvokeFilterProvider _invokeFilterProvider;
    private readonly IIoFilterProvider _ioFilterProvider;

    public RequestFilterManagementService(
        IGlobalFilterService globalFilterService,
        IInvokeFilterProvider invokeFilterProvider,
        IIoFilterProvider ioFilterProvider) {
        _globalFilterService = globalFilterService;
        _invokeFilterProvider = invokeFilterProvider;
        _ioFilterProvider = ioFilterProvider;
    }

    public IReadOnlyList<FilterProvider> GetFilters(IRequestHandlerInfo requestHandlerInfo, params object[] providers) {
        var allFilters = new List<RequestFilterInfo> {
            _ioFilterProvider.ProviderFilter(requestHandlerInfo),
            _invokeFilterProvider.ProvideFilter(requestHandlerInfo)
        };

        foreach (var filterInfo in _globalFilterService.ProviderFilters(requestHandlerInfo)) {
            allFilters.Add(filterInfo);
        }

        foreach (var attribute in requestHandlerInfo.Attributes) {
            if (attribute is IRequestFilterProvider requestFilterProvider) {
                foreach (var info in requestFilterProvider.ProviderFilters(requestHandlerInfo)) {
                    allFilters.Add(info);
                }
            }
        }
        
        foreach (var provider in providers) {
            if (provider is IRequestFilterProvider requestFilterProvider) {
                foreach (var info in requestFilterProvider.ProviderFilters(requestHandlerInfo)) {
                    allFilters.Add(info);
                }
            }
        }
        
        allFilters.Sort(
            (x,y) => x.Order - y.Order);
        
        return allFilters.Select(f => f.FilterProvider).ToArray();
    }
}
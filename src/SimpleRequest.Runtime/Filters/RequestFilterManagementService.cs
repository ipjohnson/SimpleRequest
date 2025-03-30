using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Filters;

public interface IRequestFilterManagementService {
    IReadOnlyList<FilterProvider> GetFiltersWithoutDefaults(IRequestHandlerInfo requestHandlerInfo, params object[] providers);
    
    IReadOnlyList<FilterProvider> GetFilters(
        IRequestHandlerInfo requestHandlerInfo,
        params object[] providers
        );
}

[SingletonService]
public class RequestFilterManagementService(IServiceProvider serviceProvider,
    IGlobalFilterService globalFilterService,
    IInvokeFilterProvider invokeFilterProvider,
    IIoFilterProvider ioFilterProvider)
    : IRequestFilterManagementService {

    public IReadOnlyList<FilterProvider> GetFiltersWithoutDefaults(IRequestHandlerInfo requestHandlerInfo, params object[] providers) {
        return InternalGetFilters(requestHandlerInfo, false, providers);
    }

    public IReadOnlyList<FilterProvider> GetFilters(IRequestHandlerInfo requestHandlerInfo, params object[] providers) {
        return InternalGetFilters(requestHandlerInfo, true, providers);
    }

    private IReadOnlyList<FilterProvider> InternalGetFilters(IRequestHandlerInfo requestHandlerInfo, bool includeDefaults, object[] providers) {
        var allFilters = new List<RequestFilterInfo>();

        if (includeDefaults) {
            allFilters.Add(ioFilterProvider.ProviderFilter(serviceProvider, requestHandlerInfo));
            allFilters.Add(invokeFilterProvider.ProvideFilter(serviceProvider, requestHandlerInfo));
        }
        
        foreach (var filterInfo in globalFilterService.ProviderFilters(requestHandlerInfo)) {
            allFilters.Add(filterInfo);
        }

        foreach (var attribute in requestHandlerInfo.Attributes) {
            if (attribute is IRequestFilterProvider requestFilterProvider) {
                foreach (var info in requestFilterProvider.ProviderFilters(serviceProvider, requestHandlerInfo)) {
                    allFilters.Add(info);
                }
            }
        }
        
        foreach (var provider in providers) {
            if (provider is IRequestFilterProvider requestFilterProvider) {
                foreach (var info in requestFilterProvider.ProviderFilters(serviceProvider, requestHandlerInfo)) {
                    allFilters.Add(info);
                }
            } else if (provider is IRequestFilter filter) {
                allFilters.Add(new RequestFilterInfo(_ => filter, RequestFilterOrder.Normal));
            } else if (provider is RequestFilterInfo info) {
                allFilters.Add(info);
            }
        }
        
        allFilters.Sort(
            (x,y) => x.Order.CompareTo(y.Order));
        
        return allFilters.Select(f => f.FilterProvider).ToArray();
    }
}
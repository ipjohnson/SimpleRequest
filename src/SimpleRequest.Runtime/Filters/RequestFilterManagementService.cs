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
public class RequestFilterManagementService(IServiceProvider serviceProvider,
    IGlobalFilterService globalFilterService,
    IInvokeFilterProvider invokeFilterProvider,
    IIoFilterProvider ioFilterProvider)
    : IRequestFilterManagementService {

    public IReadOnlyList<FilterProvider> GetFilters(IRequestHandlerInfo requestHandlerInfo, params object[] providers) {
        var allFilters = new List<RequestFilterInfo> {
            ioFilterProvider.ProviderFilter(serviceProvider, requestHandlerInfo),
            invokeFilterProvider.ProvideFilter(serviceProvider, requestHandlerInfo)
        };

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
            }
        }
        
        allFilters.Sort(
            (x,y) => x.Order - y.Order);
        
        return allFilters.Select(f => f.FilterProvider).ToArray();
    }
}
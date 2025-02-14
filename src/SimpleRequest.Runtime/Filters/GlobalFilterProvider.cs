using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Filters;


public interface IGlobalFilterService {
    void AddFilter(IRequestFilter filter, int order = RequestFilterOrder.Normal);
    
    
    IEnumerable<RequestFilterInfo> ProviderFilters(IRequestHandlerInfo requestHandler);
}

[SingletonService]
public class GlobalFilterService : IGlobalFilterService {
    private readonly List<Func<IRequestHandlerInfo, IEnumerable<RequestFilterInfo>>> _filterProviders = new();

    public GlobalFilterService(IEnumerable<IRequestFilterProvider> filterProviders) {
        foreach (var filterProvider in filterProviders) {
            _filterProviders.Add(filterProvider.ProviderFilters);
        }
    }
    
    public void AddFilter(IRequestFilter filter, int order = RequestFilterOrder.Normal) {
        var filterInfo = new RequestFilterInfo(_ => filter, order);
        _filterProviders.Add(_ => [filterInfo]);
    }

    public void AddFilter(Func<IRequestHandlerInfo, IRequestFilter?> filterFunc,  int order = RequestFilterOrder.Normal) {
        _filterProviders.Add(info => {
            var filter = filterFunc(info);

            if (filter == null) return [];
            var filterInfo = new RequestFilterInfo(_ => filter, order);
            return [filterInfo];
        });
    }

    public IEnumerable<RequestFilterInfo> ProviderFilters(IRequestHandlerInfo requestHandler) {
        foreach (var func in _filterProviders) {
            foreach (var filterInfo in func(requestHandler)) {
                yield return filterInfo;
            }
        }
    }
}
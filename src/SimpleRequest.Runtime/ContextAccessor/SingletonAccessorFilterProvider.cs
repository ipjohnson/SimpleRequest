using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.ContextAccessor;

public class SingletonAccessorFilterProvider : IRequestFilterProvider {
    private readonly SingletonAccessorFilter _filter;

    public SingletonAccessorFilterProvider(IContextAccessor contextAccessor) {
        _filter = new SingletonAccessorFilter(contextAccessor);
    }

    public IEnumerable<RequestFilterInfo> ProviderFilters(IServiceProvider services,IRequestHandlerInfo requestHandler) {
        yield return new RequestFilterInfo(_ => _filter, Int32.MinValue);
    }
}
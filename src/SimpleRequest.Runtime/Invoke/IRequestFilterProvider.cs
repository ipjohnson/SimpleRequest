namespace SimpleRequest.Runtime.Invoke;

public interface IRequestFilterProvider {
    IEnumerable<RequestFilterInfo> ProviderFilters(IServiceProvider services, IRequestHandlerInfo requestHandler);
}
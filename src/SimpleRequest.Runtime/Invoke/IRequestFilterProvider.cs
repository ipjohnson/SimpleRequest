namespace SimpleRequest.Runtime.Invoke;

public interface IRequestFilterProvider {
    IEnumerable<RequestFilterInfo> ProviderFilters(IRequestHandlerInfo requestHandler);
}
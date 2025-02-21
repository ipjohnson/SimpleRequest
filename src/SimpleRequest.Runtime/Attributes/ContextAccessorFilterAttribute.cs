using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.ContextAccessor;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class ContextAccessorFilterAttribute : Attribute, IRequestFilterProvider {

    public IEnumerable<RequestFilterInfo> ProviderFilters(IServiceProvider services, IRequestHandlerInfo requestHandler) {
        SingletonAccessorFilter filter = new SingletonAccessorFilter(services.GetRequiredService<IContextAccessor>());

        yield return new RequestFilterInfo(_ => filter, int.MinValue);
    }
}
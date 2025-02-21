using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.ContextAccessor;

[DependencyModule(OnlyRealm = true)]
public partial class ContextAccessorSupport : IServiceCollectionConfiguration {
    public bool ApplyToAllEndpoints { get; set; } = true;

    public void ConfigureServices(IServiceCollection services) {
        if (ApplyToAllEndpoints) {
            services.TryAddSingleton<IRequestFilterProvider, SingletonAccessorFilterProvider>();
        }
    }
}
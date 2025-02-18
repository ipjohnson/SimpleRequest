using CompiledTemplateEngine.Runtime;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRequest.Runtime;

[DependencyModule]
[CompiledTemplateEngineRuntime.Attribute]
public partial class SimpleRequestRuntime : IServiceCollectionConfiguration {

    public void ConfigureServices(IServiceCollection services) {
        services.AddLogging();
    }
}
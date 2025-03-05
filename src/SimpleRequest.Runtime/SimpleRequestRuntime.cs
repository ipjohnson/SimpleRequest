using CompiledTemplateEngine.Runtime;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Runtime;

[DependencyModule]
[CompiledTemplateEngineRuntime.Attribute]
public partial class SimpleRequestRuntime :
    IDependencyModuleFeature<ILoggingBuilderConfiguration>,
    IDependencyModuleFeature<ILoggingConfigurationImplementation>,
    ILoggingConfigurationImplementation {
    private ILoggingConfigurationImplementation? _implementation;
    
    int IDependencyModuleFeature<ILoggingConfigurationImplementation>.Order => -1000; 
    
    public void HandleFeature(IServiceCollection collection, IEnumerable<ILoggingBuilderConfiguration> features) {
        _implementation?.Configure(collection, features);
    }

    public void HandleFeature(IServiceCollection collection, IEnumerable<ILoggingConfigurationImplementation> feature) {
        _implementation = feature.Last();
    }

    public void Configure(IServiceCollection services, IEnumerable<ILoggingBuilderConfiguration> configurations) {
        DefaultLoggingHelper.Configure(services, configurations);
    }
}
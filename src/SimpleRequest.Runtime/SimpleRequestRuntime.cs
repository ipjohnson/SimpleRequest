using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;
using SimpleRequest.Runtime.Serializers.Json;

namespace SimpleRequest.Runtime;

public partial class SimpleRequestRuntimeAttribute : ISimpleRequestEntryAttribute;

[DependencyModule(GenerateFactories = true)]
public partial class SimpleRequestRuntime :
    IDependencyModuleFeature<ILoggingBuilderConfiguration>,
    IDependencyModuleFeature<ILoggingConfigurationImplementation>,
    IDependencyModuleFeature<ISystemTextJsonConfiguration>,
    IDependencyModuleFeature<IRequestResponseConfigurationProvider>, 
    ILoggingConfigurationImplementation {
    private ILoggingConfigurationImplementation? _implementation;

    public void HandleFeature(IServiceCollection collection, 
        IEnumerable<ISystemTextJsonConfiguration> features) {
        var featuresList = features.ToList();

        collection.AddSingleton<IReadOnlyList<ISystemTextJsonConfiguration>>(
            _ => featuresList);
    }

    public void HandleFeature(IServiceCollection collection, IEnumerable<IRequestResponseConfigurationProvider> feature) {
        var requestResponse = new RequestResponseConfiguration();

        foreach (var provider in feature) {
            provider.Configure(requestResponse);
        }

        collection.AddSingleton(requestResponse);
    }

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
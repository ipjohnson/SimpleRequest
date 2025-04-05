using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Serializers.Json;

public interface ISystemTextJsonSerializerOptionProvider {
    JsonSerializerOptions GetOptions();
}

[SingletonService]
public class SystemTextJsonSerializerOptionProvider(
    IReadOnlyList<ISystemTextJsonConfiguration> configurations,
    IEnumerable<IJsonTypeInfoResolver> resolvers) :
    ISystemTextJsonSerializerOptionProvider {
    private JsonSerializerOptions? _options; 
    private readonly IReadOnlyList<IJsonTypeInfoResolver> _resolvers = resolvers.ToList();

    public JsonSerializerOptions GetOptions() {
        if (_options != null) {
            return _options;
        }
        
        var options = new JsonSerializerOptions() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = true,
        };

        if (_resolvers.Count > 0) {
            options.TypeInfoResolver = JsonTypeInfoResolver.Combine(_resolvers.ToArray());
        }

        foreach (var configuration in configurations) {
            configuration.ConfigureJson(options);
        }
        
        return _options = options;
    }
}
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
    private readonly IReadOnlyList<IJsonTypeInfoResolver> _resolvers = resolvers.ToList();

    public JsonSerializerOptions GetOptions() {
        var options = new JsonSerializerOptions() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = true,
        };

        if (_resolvers.Count > 0) {
            var resolverList = new List<IJsonTypeInfoResolver>(resolvers) {
                new DefaultJsonTypeInfoResolver()
            };

            options.TypeInfoResolver = JsonTypeInfoResolver.Combine(resolverList.ToArray());
        }

        foreach (var configuration in configurations) {
            configuration.ConfigureJson(options);
        }
        
        return options;
    }
}
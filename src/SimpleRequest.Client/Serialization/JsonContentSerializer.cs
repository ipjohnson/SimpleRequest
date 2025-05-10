using System.Text.Json;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRequest.Client.Serialization;

[SingletonService]
public class JsonContentSerializer : IContentSerializer {
    private readonly JsonSerializerOptions _options;

    [ActivatorUtilitiesConstructor]
    public JsonContentSerializer(ISystemTextJsonSerializerOptions options) {
        _options = options.Options;
    }

    public JsonContentSerializer(JsonSerializerOptions options) {
        _options = options;
    }
    
    public bool IsDefault => true;

    public string? Channel => null;

    public string ContentType => "application/json";

    public bool CanSerialize(string contentType) {
        return contentType.StartsWith("application/json") || contentType.StartsWith("json");
    }

    public ValueTask<object?> DeserializeAsync(Type responseType, string contentType, Stream stream, CancellationToken cancellationToken = default) {
        return JsonSerializer.DeserializeAsync(stream, responseType);
    }

    public Task SerializeAsync(string contentType, object value, Stream stream, CancellationToken cancellationToken = default) {
        return JsonSerializer.SerializeAsync(stream, value);
    }
}
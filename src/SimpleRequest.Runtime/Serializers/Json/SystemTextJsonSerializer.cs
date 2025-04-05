using System.Text.Json;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace SimpleRequest.Runtime.Serializers.Json;

[CrossWireService(Lifetime = ServiceLifetime.Singleton)]
public class SystemTextJsonSerializer(JsonSerializerOptions options) : IContentSerializer, IJsonSerializer {

    [ActivatorUtilitiesConstructor]
    public SystemTextJsonSerializer(ISystemTextJsonSerializerOptionProvider options) : this(options.GetOptions()) { }

    public int Order => 10;
    
    public SupportedSerializerFeature Features => SupportedSerializerFeature.All;

    public bool IsDefault => true;

    public string ContentType => "application/json";

    public bool CanSerialize(string contentType) {
        return contentType.StartsWith(ContentType);
    }


    public Task SerializeAsync(Stream stream, object value, IDictionary<string, StringValues>? headers = null, CancellationToken cancellationToken = default) {
        return JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
    }

    public string Serialize(object value) {
        return JsonSerializer.Serialize(value, options);
    }

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type, IDictionary<string, StringValues>? headers = null, CancellationToken cancellationToken = default) {
        return JsonSerializer.DeserializeAsync(stream, type, options, cancellationToken);
    }

    public ValueTask<T?> DeserializeAsync<T>(Stream stream, IDictionary<string, StringValues>? headers = null, CancellationToken cancellationToken = default) {
        return JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
    }

    public object? Deserialize(string stringValue, Type type) {
        return JsonSerializer.Deserialize(stringValue, type, options);
    }

    public T? Deserialize<T>(string stringValue) {
        return JsonSerializer.Deserialize<T>(stringValue, options);
    }
}
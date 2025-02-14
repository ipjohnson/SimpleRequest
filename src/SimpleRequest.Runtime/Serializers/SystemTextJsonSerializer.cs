using System.Text.Json;
using System.Text.Json.Serialization;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Serializers;

public interface ISystemTextJsonSerializerOptionProvider {
    JsonSerializerOptions Options { get; }
}

[SingletonService]
public class SystemTextJsonSerializerOptionProvider : ISystemTextJsonSerializerOptionProvider {

    public JsonSerializerOptions Options => new JsonSerializerOptions(
    ) {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true,
    };
}

[SingletonService(ServiceType = typeof(IContentSerializer))]
[SingletonService(ServiceType = typeof(IJsonSerializer))]
public class SystemTextJsonSerializer : IContentSerializer, IJsonSerializer {
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonSerializer(ISystemTextJsonSerializerOptionProvider options) {
        _options = options.Options;
    }

    public bool IsDefault => true;

    public string ContentType  => "application/json";

    public bool CanSerialize(string contentType) {
        return contentType.StartsWith(ContentType);
    }

    public Task Serialize(Stream stream, object value, CancellationToken cancellationToken = default) {
        return JsonSerializer.SerializeAsync(stream, value, _options, cancellationToken);
    }

    public string Serialize(object value) {
        return JsonSerializer.Serialize(value, _options);
    }

    public ValueTask<object?> Deserialize(Stream stream, Type type, CancellationToken cancellationToken = default) {
        return JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken);
    }

    public ValueTask<T?> Deserialize<T>(Stream stream, CancellationToken cancellationToken = default) {
        return JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
    }

    public object? Deserialize(string stringValue, Type type) {
        return JsonSerializer.Deserialize(stringValue, type, _options);
    }

    public T? Deserialize<T>(string stringValue) {
        return JsonSerializer.Deserialize<T>(stringValue, _options);
    }
}
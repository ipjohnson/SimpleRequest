using Microsoft.Extensions.Primitives;

namespace SimpleRequest.Runtime.Serializers;

[Flags]
public enum SupportedSerializerFeature {
    Serialize = 1,
    SerializeAsync = 2,
    Deserialize = 4,
    DeserializeAsync = 8,
    
    All = Serialize | SerializeAsync | Deserialize | DeserializeAsync,
}

public interface IBaseSerializer {
    
    SupportedSerializerFeature Features { get; }
        
    Task SerializeAsync(Stream stream, object value, IDictionary<string,StringValues>? headers = null, CancellationToken cancellationToken = default);
    
    string Serialize(object value);
    
    ValueTask<object?> DeserializeAsync(Stream stream, Type type, IDictionary<string,StringValues>? headers = null, CancellationToken cancellationToken = default);
    
    ValueTask<T?> DeserializeAsync<T>( Stream stream, IDictionary<string,StringValues>? headers = null, CancellationToken cancellationToken = default);
    
    object? Deserialize(string stringValue, Type type);
    
    T? Deserialize<T>(string stringValue);
}
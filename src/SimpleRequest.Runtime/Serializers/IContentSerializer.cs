namespace SimpleRequest.Runtime.Serializers;

public interface IContentSerializer {
    bool IsDefault { get; }
    
    string ContentType { get; }
    
    bool CanSerialize(string contentType);
    
    Task Serialize(Stream stream, object value, CancellationToken cancellationToken = default);
    
    string Serialize(object value);
    
    ValueTask<object?> Deserialize(Stream stream, Type type, CancellationToken cancellationToken = default);
    
    ValueTask<T?> Deserialize<T>(Stream stream, CancellationToken cancellationToken = default);
    
    object? Deserialize(string stringValue, Type type);
    
    T? Deserialize<T>(string stringValue);
}
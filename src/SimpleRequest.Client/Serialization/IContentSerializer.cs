namespace SimpleRequest.Client.Serialization;

public interface IContentSerializer {
    int Order => 0;
    
    bool IsDefault { get; }
    
    string? Channel { get; }
    
    string ContentType { get; }
    
    bool CanSerialize(string contentType);
    
    ValueTask<object?> DeserializeAsync(Type responseType, string contentType, Stream stream, CancellationToken cancellationToken = default);
    
    Task SerializeAsync(object value, Stream stream, CancellationToken cancellationToken = default);
}
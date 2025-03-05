namespace SimpleRequest.Runtime.Serializers;

public interface IContentSerializer : IBaseSerializer {
    bool IsDefault { get; }
    
    string ContentType { get; }
    
    bool CanSerialize(string contentType);
}
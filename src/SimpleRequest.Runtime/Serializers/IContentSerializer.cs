namespace SimpleRequest.Runtime.Serializers;

public interface IContentSerializer : IBaseSerializer {
    int Order => 0;
    
    bool IsDefault { get; }
    
    string ContentType { get; }
    
    bool CanSerialize(string contentType);
}
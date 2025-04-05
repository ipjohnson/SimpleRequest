using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IContentSerializerManager {
    IContentSerializer? GetSerializer(string? contentType);
    
    IReadOnlyList<IContentSerializer> Serializers { get; }
}

public static class ContentSerializerManagerExtensions {
    public static ValueTask<T?> Deserialize<T>(this IContentSerializerManager manager, IRequestContext context) {
        var contentType = context.RequestData.ContentType;
        var serializer = manager.GetSerializer(contentType);

        if (serializer == null) {
            throw new ArgumentException($"No serializer for type {contentType} was found.");
        }

        if (context.RequestData.Body == null) {
            throw new ArgumentException("No request body was provided.");
        }

        return serializer.DeserializeAsync<T>(context.RequestData.Body!, context.RequestData.Headers, context.CancellationToken);
    }
}

[SingletonService]
public class ContentSerializerManager : IContentSerializerManager {
    private readonly IReadOnlyList<IContentSerializer> _serializers;
    private readonly IContentSerializer? _defaultSerializer;

    public ContentSerializerManager(IEnumerable<IContentSerializer> serializers) {
        var list = serializers.ToList();
        
        list.Sort((x, y) => x.Order.CompareTo(y.Order));
        
        _serializers = list;
        
        _defaultSerializer = _serializers.FirstOrDefault(x => x.IsDefault);
    }

    public IContentSerializer? GetSerializer(string? contentType) {
        if (contentType == null) {
            return _defaultSerializer;
        }
        
        foreach (var serializer in _serializers) {
            if (serializer.CanSerialize(contentType)) {
                return serializer;
            }
        }

        return _defaultSerializer;
    }

    public IReadOnlyList<IContentSerializer> Serializers => _serializers;
}

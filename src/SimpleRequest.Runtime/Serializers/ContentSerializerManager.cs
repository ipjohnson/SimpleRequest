using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IContentSerializerManager {
    IContentSerializer? GetSerializer(string contentType);
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

        return serializer.Deserialize<T>(context.RequestData.Body!, context.CancellationToken);
    }
}

[SingletonService]
public class ContentSerializerManager : IContentSerializerManager {
    private readonly IReadOnlyList<IContentSerializer> _serializers;

    public ContentSerializerManager(IEnumerable<IContentSerializer> serializers) {
        _serializers = serializers.ToList();
    }

    public IContentSerializer? GetSerializer(string contentType) {
        IContentSerializer? defaultSerializer = null;
        
        foreach (var serializer in _serializers) {
            if (serializer.CanSerialize(contentType)) {
                return serializer;
            }

            if (serializer.IsDefault && defaultSerializer == null) {
                defaultSerializer = serializer;
            }
        }
        
        return defaultSerializer;
    }
}
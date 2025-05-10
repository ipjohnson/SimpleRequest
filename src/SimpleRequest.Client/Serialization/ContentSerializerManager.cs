using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Client.Serialization;

public interface IContentSerializerManager {
    IContentSerializer? ContentSerializer(string channel);
}

[SingletonService]
public class ContentSerializerManager : IContentSerializerManager {

    public IContentSerializer? ContentSerializer(string channel) {
        return null;
    }
}
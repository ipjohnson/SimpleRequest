using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Client.Http;

public interface IHttpPathBuilder {
    void SetPath(string path);
    
    void AddQuery(string key, object? value);
    
    void AddPathValue(string id, object? value);
    
    string Build();
}

[TransientService]
public class HttpPathBuilder {
    
}
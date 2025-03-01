using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Serializers;


public interface IContentTypeHelper {
    string GetContentTypeFromExtension(string extension);
}

[SingletonService]
public class ContentTypeHelper : IContentTypeHelper {

    public string GetContentTypeFromExtension(string extension) {
        switch (extension) {
            case ".json":
                return "application/json";
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".png":
                return "image/png";
            
            default: return "text/plain";
        }
    }
}
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Serializers;


public interface IContentTypeHelper {
    string GetContentTypeFromExtension(string extension);
}

[SingletonService]
public class ContentTypeHelper : IContentTypeHelper {
    
    public string GetContentTypeFromExtension(string extension) {
        var custom = CustomContentType(extension);

        if (custom != null) {
            return custom;
        }
        
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
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            case ".svg":
                return "image/svg+xml";
            case ".xml":
                return "application/xml";
            
            default: return "text/plain";
        }
    }

    protected virtual string? CustomContentType(string extension) {
        return null;
    }
}
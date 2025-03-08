using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;

namespace SimpleRequest.SwaggerUi.Services;

public interface ISwaggerContentProvider {
    Task<ContentResult?> GetContent(string path, IRequestContext requestContext);
}

[SingletonService]
public class SwaggerContentProvider(
    ISwaggerIndexProvider indexProvider,
    ISwaggerInitializerProvider initializerProvider,
    ISwaggerCompressedFileProvider compressedFileProvider) : ISwaggerContentProvider {
    
    public Task<ContentResult?> GetContent(string path, IRequestContext requestContext) {
        if (path == "index.html") {
            return indexProvider.GetIndex(requestContext);
        }

        if (path == "swagger-initializer.js") {
            return initializerProvider.GetInitializer(requestContext);
        }
        
        return compressedFileProvider.GetCompressedFile(path, requestContext);
    }
}
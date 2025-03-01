using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.SwaggerUi.Services;

public interface ISwaggerIndexProvider {
    Task<ContentResult?> GetIndex(IRequestContext context);
}

[SingletonService]
public class SwaggerIndexProvider(
    IEmbeddedCompressedFileAccessor accessor, IContentTypeHelper helper) : ISwaggerIndexProvider {

    public async Task<ContentResult?> GetIndex(IRequestContext context) {
        var file = await accessor.ReadFile("index.html");

        return file != null ? 
            ContentResult.Ok(file, helper.GetContentTypeFromExtension(".html")) : 
            null;
    }
}
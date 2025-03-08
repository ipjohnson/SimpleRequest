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
    IEmbeddedCompressedFileAccessor accessor,
    IContentTypeHelper helper,
    ISwaggerIndexTransformer transformer) : ISwaggerIndexProvider {

    public async Task<ContentResult?> GetIndex(IRequestContext context) {
        var file = await accessor.ReadFile("index.html");

        return file != null ? 
            ContentResult.Ok(await transformer.Transform(file),
                helper.GetContentTypeFromExtension(".html")) : 
            null;
    }
}
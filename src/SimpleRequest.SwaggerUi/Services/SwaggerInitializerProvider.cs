using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.SwaggerUi.Services;

public interface ISwaggerInitializerProvider {
    Task<ContentResult?> GetInitializer(IRequestContext context);
}

[SingletonService]
public class SwaggerInitializerProvider(
    IEmbeddedCompressedFileAccessor accessor,
    IContentTypeHelper helper,
    ISwaggerInitializerTransformer transformer) : ISwaggerInitializerProvider {

    public async Task<ContentResult?> GetInitializer(IRequestContext context) {
        var file = await accessor.ReadFile("swagger-initializer.js");

        return file != null ? 
            ContentResult.Ok(await transformer.Transform(file),
                helper.GetContentTypeFromExtension(".js")) : 
            null;
    }
}
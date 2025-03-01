using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.SwaggerUi.Services;

public interface ISwaggerCompressedFileProvider {
    Task<ContentResult?> GetCompressedFile(string filePath, IRequestContext context);
}

[SingletonService]
public class SwaggerCompressedFileProvider(
    IEmbeddedCompressedFileAccessor accessor, IContentTypeHelper helper) : ISwaggerCompressedFileProvider {

    public async Task<ContentResult?> GetCompressedFile(
        string filePath, IRequestContext context) {
        var file = await accessor.ReadFile(filePath);

        return file != null ? 
            ContentResult.Ok(file, helper.GetContentTypeFromExtension(Path.GetExtension(filePath))) : 
            null;
    }
}
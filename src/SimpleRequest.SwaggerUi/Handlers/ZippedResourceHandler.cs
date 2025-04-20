using SimpleRequest.Caching;
using SimpleRequest.Models.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;
using SimpleRequest.SwaggerUi.Services;

namespace SimpleRequest.SwaggerUi.Handlers;

public class ZippedResourceHandler(ISwaggerContentProvider provider) {
    [Get("/swagger/{resourceName}")]
    [ResponseCache]
    public Task<ContentResult?> FindResource(string resourceName, IRequestContext context) {
        return provider.GetContent(resourceName, context);
    }
}
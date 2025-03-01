using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;
using SimpleRequest.SwaggerUi.Services;
using SimpleRequest.Web.Runtime.Attributes;

namespace SimpleRequest.SwaggerUi.Handlers;

public class ZippedResourceHandler(ISwaggerContentProvider provider) {
    [Get("/swagger/{resourceName}")]
    public Task<ContentResult?> FindResource(string resourceName, IRequestContext context) {
        return provider.GetContent(resourceName, context);
    }
}
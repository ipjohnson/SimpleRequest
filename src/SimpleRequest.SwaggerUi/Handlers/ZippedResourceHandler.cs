using SimpleRequest.Runtime.Models;
using SimpleRequest.Web.Runtime.Attributes;

namespace SimpleRequest.SwaggerUi.Handlers;

public class ZippedResourceHandler {
    [Get("/swagger/{resourceName}")]
    public async Task<ContentResult?> FindResource(string resourceName) {
        return null;
    }
}
using Microsoft.OpenApi.Models;
using SimpleRequest.Swagger.Services;
using SimpleRequest.Web.Runtime.Attributes;

namespace SimpleRequest.Swagger.Handlers;

public class IndexHandler(IOpenApiDocumentGenerator generator) {

    [Get("/swagger/v1/swagger.json")]
    public Task<OpenApiDocument> GetDocument() {
        return generator.GenerateDocument();
    }
}
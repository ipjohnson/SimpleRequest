using DependencyModules.Runtime.Attributes;
using Microsoft.OpenApi.Models;

namespace SimpleRequest.Swagger.Services;

public interface IOpenApiDocumentGenerator {
    Task<OpenApiDocument> GenerateDocument();
}

[SingletonService]
public class OpenApiDocumentGenerator : IOpenApiDocumentGenerator {
    public async Task<OpenApiDocument> GenerateDocument() {
        return new OpenApiDocument();
    }
}
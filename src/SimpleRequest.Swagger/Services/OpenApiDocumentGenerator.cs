using DependencyModules.Runtime.Attributes;
using Microsoft.OpenApi.Models;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Swagger.Services;

public interface IOpenApiDocumentGenerator {
    Task<OpenApiDocument> GenerateDocument();
}

[SingletonService]
public class OpenApiDocumentGenerator(
    IRequestHandlerLocator handlerLocator,
    IOpenApiSchemaGenerator schemaGenerator,
    IContentSerializerManager contentSerializerManager) : IOpenApiDocumentGenerator {
    private IReadOnlyList<string> _contentTypes = 
        contentSerializerManager.Serializers.Select(s => s.ContentType).ToList();
    
    public async Task<OpenApiDocument> GenerateDocument() {
        
        var openApiDocument = new OpenApiDocument {
            Paths = new OpenApiPaths()
        };
        
        AddOperations(openApiDocument);
        schemaGenerator.PopulateSchemaComponent(openApiDocument);
        
        return openApiDocument;
    }

    private void AddOperations(OpenApiDocument openApiDocument) {
        var handlers = handlerLocator.GetHandlers().ToList();

        var routeMatch = new Dictionary<string, List<IRequestHandler>>();
        
        foreach (var handler in handlers) {
            if (handler.RequestHandlerInfo.Path.StartsWith("/swagger/")) {
                continue;
            }
            
            if (!routeMatch.TryGetValue(handler.RequestHandlerInfo.Path, out var routes)) {
                routes = new List<IRequestHandler>();
                routeMatch[handler.RequestHandlerInfo.Path] = routes;
            }
            
            routes.Add(handler);
        }
        
        ProcessRoutes(openApiDocument, routeMatch);
    }

    private void ProcessRoutes(OpenApiDocument openApiDocument, Dictionary<string, List<IRequestHandler>> routeMatch) {
        foreach (var kvp in routeMatch) {
            ProcessRouteHandlers(openApiDocument, kvp.Key, kvp.Value);
        }
    }

    private void ProcessRouteHandlers(OpenApiDocument openApiDocument, string kvpKey, List<IRequestHandler> kvpValue) {
        var operations = new Dictionary<OperationType,OpenApiOperation>();

        foreach (var handler in kvpValue) {
            ProcessHandler(openApiDocument, operations, handler);
        }
        
        var pathItem = new OpenApiPathItem {
            Operations = operations
        };

        openApiDocument.Paths[kvpKey] = pathItem;
    }

    private void ProcessHandler(
        OpenApiDocument openApiDocument, Dictionary<OperationType, OpenApiOperation> operations, IRequestHandler handler) {
        Enum.TryParse<OperationType>(handler.RequestHandlerInfo.Method, out var type);
        
        var operation = new OpenApiOperation {
            Parameters = GenerateParameters(openApiDocument, handler, type),
        };
        
        SetupBodyParameter(handler, operation);
        SetupResponse(handler, operation);
        
        operations[type] = operation;
    }

    private void SetupResponse(IRequestHandler handler, OpenApiOperation operation) {
        if (handler.RequestHandlerInfo.InvokeInfo.ResponseType == typeof(void)) {
            return;
        }
        
        operation.Responses = new OpenApiResponses();
        SetupSuccessResponse(handler, operation);
        SetupNotFoundResponse(handler, operation);
    }

    private void SetupNotFoundResponse(IRequestHandler handler, OpenApiOperation operation) {
        var missingResponse = handler.RequestHandlerInfo.NullResponseStatus ?? 404;
        operation.Responses.Add(missingResponse.ToString(), 
            new OpenApiResponse {
                Description = "Resource Not Found",
            });
    }

    private void SetupSuccessResponse(IRequestHandler handler, OpenApiOperation operation) {
        var successResponse = handler.RequestHandlerInfo.SuccessStatus ?? 200;
        
        var content = new Dictionary<string, OpenApiMediaType>();
        var successResponseType = new OpenApiMediaType {
            Schema = schemaGenerator.GetSchemaType(handler.RequestHandlerInfo.InvokeInfo.ResponseType)
        };

        foreach (var contentType in _contentTypes) {
            content.Add(contentType, successResponseType);
        }
        
        operation.Responses.Add(successResponse.ToString(), 
            new OpenApiResponse {
                Description = "OK",
                Content = content,
            });
    }

    private void SetupBodyParameter(IRequestHandler handler, OpenApiOperation operation) {
        var bodyParameter = handler.RequestHandlerInfo.InvokeInfo.Parameters.FirstOrDefault(
            p => p.BindingType == ParameterBindType.Body);

        if (bodyParameter != null) {
            var contentDictionary = new Dictionary<string, OpenApiMediaType>();
            
            var mediaType = new OpenApiMediaType {
                Schema = schemaGenerator.GetSchemaType(bodyParameter.Type)
            };

            foreach (var contentType in _contentTypes) {
                contentDictionary.Add(contentType, mediaType);
            }
            
            operation.RequestBody = new OpenApiRequestBody {
                Content = contentDictionary,
            };
        }
    }

    private List<OpenApiParameter> GenerateParameters(OpenApiDocument openApiDocument, IRequestHandler handler, OperationType result) {
        var parameters = new List<OpenApiParameter>();

        foreach (var parameter in handler.RequestHandlerInfo.InvokeInfo.Parameters) {
            ParameterLocation? location = null;
            
            switch (parameter.BindingType) {
                case ParameterBindType.Path:
                    location = ParameterLocation.Path; 
                    break;
                case ParameterBindType.Header:
                    location = ParameterLocation.Header;
                    break;
                case ParameterBindType.QueryString:
                    location = ParameterLocation.Query;
                    break;
            }

            if (location == null) {
                continue;
            }
            
            var schema = schemaGenerator.GetSchemaType(parameter.Type);
            var name = string.IsNullOrEmpty(parameter.BindingName) ?
                parameter.Name : parameter.BindingName;
            
            var apiParameter = new OpenApiParameter {
                Name = name,
                Schema = schema,
                In = location
            };
            
            parameters.Add(apiParameter);
        }
        
        return parameters;
    }
}
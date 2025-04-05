using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.JsonRpc.Models;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.JsonRpc.Impl;

public interface IJsonRpcRequestProcessor {

    Task HandleRequest(IRequestContext context, string jsonRpcTag, bool processInParallel);
}

public class JsonRpcRequestProcessor(
    IJsonRpcParameterBinder binder,
    IJsonRpcHandlerLocator rpcHandlerLocator) : IJsonRpcRequestProcessor {
    private ConcurrentDictionary<string, IReadOnlyList<IRequestHandlerProvider>> _handlers = new();
    
    public async Task HandleRequest(IRequestContext context, string jsonRpcTag, bool processInParallel) {
        var handlers = _handlers.GetOrAdd(jsonRpcTag, 
            s => context.ServiceProvider.GetKeyedServices<IRequestHandlerProvider>(s).ToList());

        var jsonRpcRequest = await ParseJsonRequest(context);
        
        if (jsonRpcRequest == null) {
            await RespondWithInvalidJsonError(context);
        }
        else if (jsonRpcRequest.GetValueKind() == JsonValueKind.Array) {
            await HandleBatchRequest(context, jsonRpcRequest);
        }
        else if (jsonRpcRequest.GetValueKind() == JsonValueKind.Object) {
            await HandleSingleRequest(context, jsonRpcRequest);
        }
        else {
            await RespondWithInvalidJsonError(context);
        }
    }

    private async Task HandleBatchRequest(IRequestContext context, JsonNode jsonRpcRequest) {
        var responseList = new List<JsonRpcResponseModel>();
        
        foreach (var jsonNode in jsonRpcRequest.AsArray()) {
            if (jsonNode?.GetValueKind() == JsonValueKind.Object) {
                var response = await InvokeJsonRpcMethod(context, jsonNode.AsObject(), true);
                responseList.Add(response);
            }
            else {
                
            }
        }
    }

    private async Task HandleSingleRequest(IRequestContext context, JsonNode jsonRpcRequest) {
        context.ResponseData.ResponseValue = await InvokeJsonRpcMethod(context, jsonRpcRequest.AsObject(), false);
    }

    private async Task RespondWithInvalidJsonError(IRequestContext context) {
        context.ResponseData.ResponseValue = new JsonRpcErrorModel {
            Code = -32700,
            Message = "Invalid JSON"
        };
    }

    private async Task<JsonRpcResponseModel> InvokeJsonRpcMethod(IRequestContext context, JsonObject jsonNode, bool useNewScope) {
        
        var jsonRequest = GetJsonRequest(context, jsonNode);

        if (jsonRequest == null) {
            return new JsonRpcResponseModel {
                Error = new JsonRpcErrorModel {
                    Code = -32600,
                    Message = "Invalid Request"
                },
            };
        }
        
        var serviceProvider = context.ServiceProvider;
        IServiceScope? scope = null;
        
        if (useNewScope) {
            scope = serviceProvider.CreateScope();
            serviceProvider = scope.ServiceProvider;
        }
        
        var cloneContext = CloneContext(context, serviceProvider, jsonRequest);
        var handler = rpcHandlerLocator.Locate(cloneContext);

        if (handler == null) {
            return new JsonRpcResponseModel {
                Error = new JsonRpcErrorModel {
                    Code = -32601,
                    Message = "Invalid Request"
                },
            };
        }

        cloneContext.RequestHandlerInfo = handler.RequestHandlerInfo;

        if (!binder.TryBind(cloneContext, jsonRequest.Params)) {
            return new JsonRpcResponseModel {
                Error = new JsonRpcErrorModel {
                    Code = -32602,
                    Message = "Invalid params"
                },
            };
        }
        
        await handler.Invoke(cloneContext);
        
        var response = ConstructResponse(cloneContext, jsonRequest);
        
        scope?.Dispose();

        return response;
    }

    private JsonRpcResponseModel ConstructResponse(IRequestContext context, JsonRequest jsonRequest) {
        if (context.ResponseData.ExceptionValue != null) {
            return new JsonRpcResponseModel {
                Error = new JsonRpcErrorModel {
                    Code = -32603,
                    Message = context.ResponseData.ExceptionValue.Message,
                    Data = context.ResponseData.ExceptionValue.Data
                },
                Id = jsonRequest.Id
            };
        }

        return new JsonRpcResponseModel {
            Result = context.ResponseData.ResponseValue,
            Id = jsonRequest.Id
        };
    }

    private IRequestContext CloneContext(IRequestContext context, IServiceProvider serviceProvider, JsonRequest jsonRequest) {
        var clone = context.Clone(serviceProvider, context.RequestData.Clone(
            jsonRequest.Method, "POST", "application/json"));
        
        return clone;
    }

    private record JsonRequest(string JsonRpc, string Method, JsonNode? Params, object? Id);
    
    private JsonRequest? GetJsonRequest(IRequestContext context, JsonObject jsonObject) {
        if (!jsonObject.TryGetPropertyValue("jsonrpc", out var jsonVersion) || 
            jsonVersion?.ToString() != "2.0") {
            return null;
        }
        
        if (!jsonObject.TryGetPropertyValue("method", out var method) ||
            method!.GetValueKind() != JsonValueKind.String) {
            return null;
        }
        
        jsonObject.TryGetPropertyValue("params", out var paramsNode);
        jsonObject.TryGetPropertyValue("id", out var idNode);
        
        object? id = null;

        if (idNode != null) {
            if (idNode.GetValueKind() == JsonValueKind.String) {
                id = idNode.ToString();
            }
            else if (idNode.GetValueKind() == JsonValueKind.Number) {
                id = idNode.GetValue<int>();
            }
            else {
                return null;
            }
        }
        
        return new JsonRequest(
            jsonVersion.ToString(),
            method.ToString(),
            paramsNode, 
            id);
    }

    private bool ValidateJsonRpcProperty(JsonObject jsonObject) {
        if (!jsonObject.TryGetPropertyValue("jsonrpc", out var jsonVersion) || 
            jsonVersion?.ToString() != "2.0") {
            return false;
        }
        
        return true;
    }

    private async Task<JsonNode?> ParseJsonRequest(IRequestContext context) {
        if (context.RequestData.Body == null) {
            return null;
        }

        try {
            return await JsonNode.ParseAsync(context.RequestData.Body);
        }
        catch (Exception e) {
            // todo: add logging
            return null;
        }
    }
}
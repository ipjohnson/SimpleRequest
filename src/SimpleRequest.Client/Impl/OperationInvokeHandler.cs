using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Client.Filters;
using SimpleRequest.Client.Model;
using SimpleRequest.Client.Serialization;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;

public record InvokeDelegateInfo<TRequest, TResponse>(
    string ChannelName,
    IOperationInfo Operation,
    IPathBuilder PathBuilder,
    IContentSerializer Serializer,
    IOperationFilter[] OperationFilters,
    ITransportFilter<TRequest, TResponse>[] TransportFilters);

public interface IOperationInvokeHandler<TRequest, TResponse> {
    Task<T?> InvokeDelegate<T>(
        InvokeDelegateInfo<TRequest, TResponse> info,
        IServiceProvider serviceProvider,
        IOperationParameters parameters);
}

[SingletonService(Using = RegistrationType.Try)]
public class OperationInvokeHandler<TRequest, TResponse> : IOperationInvokeHandler<TRequest, TResponse> {
    
    public async Task<T?> InvokeDelegate<T>(
        InvokeDelegateInfo<TRequest, TResponse> info,
        IServiceProvider serviceProvider,
        IOperationParameters parameters) {

        var operationRequest = new OperationRequest(
            info.Operation,
            parameters, 
            new Dictionary<string, StringValues>());

        var operationResponse = 
            new OperationResponse(info.Operation, null, new Dictionary<string, StringValues>());
        
        var context = new FilterContext<TRequest, TResponse>(
            info.ChannelName,
            info.PathBuilder,
            info.Serializer,
            serviceProvider, 
            operationRequest,
            operationResponse,
            info.OperationFilters, 
            info.TransportFilters);
        
        await context.Next();

        if (operationResponse.Result is T tResult) {
            return tResult;
        }
        
        return default;
    }
}
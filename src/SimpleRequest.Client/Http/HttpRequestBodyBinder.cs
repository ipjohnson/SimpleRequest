using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.ObjectPool;
using SimpleRequest.Client.Filters;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Http;

public interface IHttpRequestBodyBinder {
    Task BindBody(
        ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context,
        IOperationParameterInfo parameterInstance);
}

[SingletonService]
public class HttpRequestBodyBinder(ObjectPool<MemoryStream> pool) : IHttpRequestBodyBinder {

    public async Task BindBody(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context, IOperationParameterInfo parameterInstance) {
        if (context.TransportRequest == null) { 
            throw new Exception("Transport request is null.");   
        }
        
        var memoryStream = pool.Get();
        
        var value = context.OperationRequest.Parameters.Get<object>(parameterInstance.Index);
        
        await context.ContentSerializer.SerializeAsync(value!, memoryStream);
        
        memoryStream.Position = 0;
        context.TransportRequest.Content = new StreamContent(memoryStream);
    }
}
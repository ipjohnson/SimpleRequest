using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;

namespace SimpleRequest.Client.Http;

public interface IHttpResponseBodyBinder {
    Task BindResponse(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context);
}

[SingletonService]
public class HttpResponseBodyBinder : IHttpResponseBodyBinder {

    public async Task BindResponse(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context) {
        if (context.TransportResponse == null) {
            throw new Exception("TransportResponse is null");
        }
        
        var contentType = context.TransportResponse.Content.Headers.ContentType?.MediaType;
        
        if (contentType != null && !context.ContentSerializer.CanSerialize(contentType)) {
            throw new Exception($"Content type {contentType} is not supported");
        }
        
        context.OperationResponse.StatusCode = (int)context.TransportResponse.StatusCode;

        if (context.OperationResponse.StatusCode is >= 200 and < 300) {
            var response = await context.ContentSerializer.DeserializeAsync(
                context.OperationRequest.Operation.ResponseType,
                contentType ?? "",
                await context.TransportResponse.Content.ReadAsStreamAsync());

            context.OperationResponse.Result = response;
        }
    }
}
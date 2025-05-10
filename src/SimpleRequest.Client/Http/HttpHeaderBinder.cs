using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;
using SimpleRequest.Client.Serialization;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Http;

public interface IHttpHeaderBinder {
    void BindHeader(
        IContentSerializer contentSerializer,
        ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context,
        IOperationParameterInfo parameterInstance);
}

[SingletonService]
public class HttpHeaderBinder : IHttpHeaderBinder {

    public void BindHeader(
        IContentSerializer contentSerializer,
        ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context,
        IOperationParameterInfo parameterInstance) {
        context.TransportRequest?.Headers.TryAddWithoutValidation(
            parameterInstance.BindingName, 
            context.OperationRequest.Parameters.Get<object>(parameterInstance.Index)?.ToString());
    }
}
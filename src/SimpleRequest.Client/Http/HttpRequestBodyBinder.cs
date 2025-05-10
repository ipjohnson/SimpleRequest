using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Http;

public interface IHttpRequestBodyBinder {
    Task BindBody(
        ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context,
        IOperationParameterInfo parameterInstance);
}

[SingletonService]
public class HttpRequestBodyBinder : IHttpRequestBodyBinder {

    public Task BindBody(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context, IOperationParameterInfo parameterInstance) {
        throw new NotImplementedException();
    }
}
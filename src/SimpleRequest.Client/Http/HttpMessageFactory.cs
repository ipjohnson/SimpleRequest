using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;

namespace SimpleRequest.Client.Http;

public interface IHttpMessageFactory {
    HttpRequestMessage GenerateRequestMessage(
        ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context);
}

[SingletonService(Using = RegistrationType.Try)]
public class HttpMessageFactory : IHttpMessageFactory {

    public HttpRequestMessage GenerateRequestMessage(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context) {
        return new HttpRequestMessage();
    }
}
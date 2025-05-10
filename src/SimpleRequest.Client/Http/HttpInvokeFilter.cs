using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Http;

[SingletonService(Using = RegistrationType.Try)]
public class HttpInvokeFilter(IHttpClientSendService sendService) : 
    ITransportInvokeFilter<HttpRequestMessage, HttpResponseMessage> {

    public int Order => Int32.MaxValue;

    public bool SupportOperation(string channel, IOperationInfo operation) {
        return true;
    }

    public async Task Invoke(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context) {
        context.TransportResponse = await sendService.SendAsync(context);
    }
}
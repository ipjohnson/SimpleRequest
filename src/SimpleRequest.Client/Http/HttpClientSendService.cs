using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;

namespace SimpleRequest.Client.Http;

public interface IHttpClientSendService {
    Task<HttpResponseMessage> SendAsync(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context);
}

[SingletonService]
public class HttpClientSendService(IHttpClientFactory clientFactory) : IHttpClientSendService {

    public async Task<HttpResponseMessage> SendAsync(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context) {
        if (context.TransportRequest == null) {
            throw new ArgumentNullException(nameof(context));
        }

        var client = clientFactory.CreateClient(context.ChannelName);
        
        return await client.SendAsync(context.TransportRequest);
    }
}
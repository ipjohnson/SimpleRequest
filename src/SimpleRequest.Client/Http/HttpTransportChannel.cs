using SimpleRequest.Client.Impl;
using SimpleRequest.Client.Serialization;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Http;

public class HttpTransportChannel(
    IContentSerializer contentSerializer,
    IInvokeStrategyBuilder<HttpRequestMessage,HttpResponseMessage> invokeStrategyBuilder,
    string channelName) : ITransportChannel {

    public Task Initialize() {
        return Task.CompletedTask;
    }

    public string ChannelName => channelName;

    public InvokeDelegate<T?> GetInvokeDelegate<T>(IOperationInfo operation) {
        
        return invokeStrategyBuilder.Build<T>(channelName, operation, contentSerializer);
    }
}
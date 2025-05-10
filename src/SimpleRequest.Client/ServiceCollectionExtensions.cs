using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Client.Http;
using SimpleRequest.Client.Impl;
using SimpleRequest.Client.Serialization;

namespace SimpleRequest.Client;

public static class ServiceCollectionExtensions {
    public static IHttpClientBuilder AddHttpTransport(this IServiceCollection services, string url, IContentSerializer? serializer = null) {
        return AddHttpTransport<DefaultClientAttribute>(services, url);
    }
    
    public static IHttpClientBuilder AddHttpTransport<T>(this IServiceCollection serviceCollection, string url, IContentSerializer? serializer = null)
        where T : Attribute, ITransportChannelAttribute {
        var name = typeof(T).Name.Replace("Attribute", "");
        
        serviceCollection.AddKeyedSingleton(name, HttpTransportChannelProvider);
        
        return serviceCollection.AddHttpClient(name).
            ConfigureHttpClient(c => c.BaseAddress = new Uri(url));
    }

    private static ITransportChannel HttpTransportChannelProvider(IServiceProvider provider, object? key, IContentSerializer? serializer) {
        if (serializer == null) {
            serializer = 
                provider.GetRequiredService<IContentSerializerManager>().ContentSerializer(
                    key?.ToString() ?? "DefaultClient");

            if (serializer == null) {
                throw new Exception($"No serializer for {key?.ToString() ?? "DefaultClient"}");
            }
        }
        
        return new HttpTransportChannel(
            serializer,
            provider.GetRequiredService<IInvokeStrategyBuilder<HttpRequestMessage, HttpResponseMessage>>(),
            (string)key!
        );
    }
}
using System.Collections.Concurrent;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.JsonRpc.Impl;

public interface IJsonRpcHandlerLocator {
    IRequestHandler? Locate(string jsonRpcPath, IRequestContext context, JsonRequest jsonRequest, string[] tags);
}

[SingletonService]
public class JsonRpcHandlerLocator(IServiceProvider serviceProvider) : IJsonRpcHandlerLocator {
    private ConcurrentDictionary<string, IReadOnlyList<IRequestHandlerProvider>> _handlers = new();
    
    public IRequestHandler? Locate(string jsonRpcPath, IRequestContext context, JsonRequest jsonRequest, string[] tags) {
        for (var i = 0; i < tags.Length; i++) {
            var pathKey = "json-rpc:" + tags[i];
            var handlers = _handlers.GetOrAdd(pathKey,
                s => serviceProvider.GetKeyedServices<IRequestHandlerProvider>(s).ToList());

            foreach (var provider in handlers) {
                var handler = provider.GetRequestHandler(context);

                if (handler != null)
                    return handler;
            }
        }
        
        return null;
    }
}
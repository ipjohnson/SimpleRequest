using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.JsonRpc.Impl;

public interface IJsonRpcHandlerLocator {
    IRequestHandler? Locate(IRequestContext context);
}

[SingletonService]
public class JsonRpcHandlerLocator : IJsonRpcHandlerLocator {

    public IRequestHandler? Locate(IRequestContext context) {
        return null;
    }
}
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.ContextAccessor;


public class SingletonAccessorFilter(IContextAccessor contextAccessor) : IRequestFilter {

    public Task Invoke(IRequestChain requestChain) {
        contextAccessor.Context = requestChain.Context;
        
        return requestChain.Next();
    }
}
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace TestApp.FunctionHandlers.Filters;

[RequestFilter(Reuse = true, Order = SomeClass.OrderValue)]
public class CustomFilter : IRequestFilter {
    
    public Task Invoke(IRequestChain requestChain) {
        
        
        return requestChain.Next();
    }
}

public static class SomeClass {
    public const int OrderValue = 10;
}
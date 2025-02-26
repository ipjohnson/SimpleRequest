using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Impl;

public class SimpleRequestRoutingHandler(IHttpContextMapper mapper, IRequestInvocationEngine engine) {

    public Task Handle(HttpContext context) {
        var requestContext = mapper.MapContext(context);
        
        return engine.Invoke(requestContext);
    }
    
}
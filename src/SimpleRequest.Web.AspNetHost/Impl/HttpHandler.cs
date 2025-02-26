using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Impl;

public class HttpHandler(IHttpContextMapper mapper, IRequestHandler requestHandler) {

    public Task Handle(HttpContext context) {
        var requestContext = mapper.MapContext(context);
        
        return requestHandler.Invoke(requestContext);
    }
}
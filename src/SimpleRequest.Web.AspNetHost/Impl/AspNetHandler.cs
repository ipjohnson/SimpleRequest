using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Impl;

public interface IAspNetHandler {
    Task Handle(HttpContext context, RequestDelegate next);
}

[SingletonService]
public class AspNetHandler(IHttpContextMapper mapper, IRequestInvocationEngine invocationEngine) : IAspNetHandler {

    public async Task Handle(HttpContext context, RequestDelegate next) {
        var requestContext = mapper.MapContext(context);
        
        context.Response.ContentType = requestContext.ResponseData.ContentType;
        context.Response.StatusCode = requestContext.ResponseData.Status ?? 200;
        
        await invocationEngine.Invoke(requestContext);

        await context.Response.CompleteAsync();
    }
}
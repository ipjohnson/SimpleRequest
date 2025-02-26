using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Impl;

public interface IHttpRouteMapper {
    WebApplication MapRoutes(WebApplication app, bool useAspNetRouting);
}

[SingletonService]
public class HttpRouteMapper : IHttpRouteMapper {
    
    public WebApplication MapRoutes(WebApplication app, bool useAspNetRouting) {
        
        if (useAspNetRouting) {
            MapUsingAspNetRouting(app);
        }
        else {
            MapUsingInternalRouting(app);
        }
        
        return app;
    }

    private void MapUsingInternalRouting(WebApplication app) {
        var mapper = app.Services.GetRequiredService<IHttpContextMapper>();
        var requestEngine = app.Services.GetRequiredService<IRequestInvocationEngine>();
        
        app.Use(_ => new SimpleRequestRoutingHandler(mapper, requestEngine).Handle);
    }

    private void MapUsingAspNetRouting(WebApplication app) {
        var mapper = app.Services.GetRequiredService<IHttpContextMapper>();
        var providers = app.Services.GetServices<IRequestHandlerProvider>();

        foreach (var provider in providers) {
            foreach (var handler in provider.GetHandlers()) {
                
                app.MapMethods(
                    handler.RequestHandlerInfo.Path,
                    [handler.RequestHandlerInfo.Method],
                    (RequestDelegate)new HttpHandler(mapper, handler).Handle
                );
            }
        }
    }
}
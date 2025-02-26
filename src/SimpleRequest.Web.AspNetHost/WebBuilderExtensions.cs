using SimpleRequest.Web.AspNetHost.Impl;

namespace SimpleRequest.Web.AspNetHost;

public static class WebBuilderExtensions {
    public static WebApplication UseSimpleRequest(this WebApplication app, bool useAspNetRouting = true) {

        var mapper = app.Services.GetRequiredService<IHttpRouteMapper>();
        
        return mapper.MapRoutes(app, useAspNetRouting);
    }
}
using SimpleRequest.Web.Runtime.Attributes;

namespace TestApp.WebHandlers.Handlers.RouteHandlers;

public class RouteTestHandler {
    [Get("/routing-test/{route}")]
    public string CatchAllRoute(string route) {
        return route;
    }

    [Get("/routing-test/{route}/{value}")]
    public string NestedWildCardRoute(string route, string value) {
        return route + "/" + value;
    }
}
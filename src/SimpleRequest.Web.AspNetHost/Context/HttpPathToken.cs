using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpPathToken(RouteValueDictionary requestRouteValues) : IPathTokenCollection {

    public int Count => requestRouteValues.Count;

    public object? Get(string key) {
        requestRouteValues.TryGetValue(key, out var value);
        
        return value;
    }

    public void Set(string key, object value) {
        requestRouteValues[key] = value;
    }
}
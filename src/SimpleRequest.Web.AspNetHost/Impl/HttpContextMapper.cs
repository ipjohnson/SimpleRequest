using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;
using SimpleRequest.Web.AspNetHost.Context;

namespace SimpleRequest.Web.AspNetHost.Impl;

public interface IHttpContextMapper {
    IRequestContext MapContext(HttpContext context);
} 

[SingletonService]
public class HttpContextMapper(
    
    IMetricLoggerProvider metricLoggerProvider,
    DataServices requestServices) : IHttpContextMapper {
    
    public IRequestContext MapContext(HttpContext context) {
        var request = MapRequest(context);
        var response = MapResponse(context);
        
        return new HttpRequestContext(context,
                request,response,
                metricLoggerProvider.CreateLogger(),
                context.RequestServices.GetRequiredService<IRequestLogger>(),
                requestServices
        );
    }

    private IResponseData MapResponse(HttpContext context) {
        return new HttpResponseData(context.Response);
    }

    private IRequestData MapRequest(HttpContext context) {
        return new HttpRequestData(context.Request);
    }
}
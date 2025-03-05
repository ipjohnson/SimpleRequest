using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpRequestContext : IRequestContext {
    private HttpContext _httpContext;

    public HttpRequestContext(
        HttpContext httpContext, 
        IRequestData requestData, 
        IResponseData responseData, 
        IMetricLogger metricLogger, 
        IRequestLogger requestLogger, 
        RequestServices requestServices) {
        _httpContext = httpContext;
        RequestData = requestData;
        ResponseData = responseData;
        MetricLogger = metricLogger;
        RequestLogger = requestLogger;
        RequestServices = requestServices;
    }

    public IServiceProvider ServiceProvider => _httpContext.RequestServices;

    public IRequestHandlerInfo? RequestHandlerInfo { get; set; }

    public IInvokeParameters? InvokeParameters { get; set; }

    public IRequestData RequestData { get; }

    public IResponseData ResponseData { get; }

    public IMetricLogger MetricLogger { get; }

    public IRequestLogger RequestLogger { get; }

    public RequestServices RequestServices { get; }

    public CancellationToken CancellationToken => _httpContext.RequestAborted;

    public IRequestContext Clone(IServiceProvider? serviceProvider = null) {
        return new HttpRequestContext(
            _httpContext,
            RequestData.Clone(),
            ResponseData.Clone(),
            MetricLogger.Clone(),
            RequestLogger,
            RequestServices
        );
    }
}
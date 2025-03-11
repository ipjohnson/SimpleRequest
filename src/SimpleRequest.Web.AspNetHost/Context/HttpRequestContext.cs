using System.Collections;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Utilities;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpRequestContextItem(IDictionary<object,object?> items) : IRequestContextItems {

    public IEnumerator<KeyValuePair<object, object?>> GetEnumerator() {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerable<object> Keys => items.Keys;

    public int Count => items.Count;

    public object? Get(object key) => items.GetValueOrDefault(key);

    public void Set(object key, object value) => items[key] = value;

    public IRequestContextItems Clone() {
        return new HttpRequestContextItem(new Dictionary<object, object?>(items));
    }
}

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

    public IRequestContextItems Items => new HttpRequestContextItem(_httpContext.Items);

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
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
    private IServiceProvider _serviceProvider;
    private CancellationToken _cancellationToken;
    private IRequestContextItems? _items;

    public HttpRequestContext(
        HttpContext httpContext, 
        IRequestData requestData, 
        IResponseData responseData, 
        IMetricLogger metricLogger, 
        IRequestLogger requestLogger, 
        DataServices requestServices) {
        _httpContext = httpContext;
        RequestData = requestData;
        ResponseData = responseData;
        MetricLogger = metricLogger;
        RequestLogger = requestLogger;
        RequestServices = requestServices;
        _serviceProvider = httpContext.RequestServices;
        _cancellationToken = httpContext.RequestAborted;
    }

    public IServiceProvider ServiceProvider => _serviceProvider;

    public IRequestHandlerInfo? RequestHandlerInfo { get; set; }

    public IInvokeParameters? InvokeParameters { get; set; }

    public IRequestData RequestData { get; }

    public IResponseData ResponseData { get; }

    public IMetricLogger MetricLogger { get; }

    public IRequestLogger RequestLogger { get; }

    public DataServices RequestServices { get; }

    public CancellationToken CancellationToken => _cancellationToken;

    public IRequestContextItems Items => _items ??= new HttpRequestContextItem(_httpContext.Items);

    public IRequestContext Clone(IServiceProvider? serviceProvider = null, IRequestData? requestData = null, IResponseData? responseData = null, IRequestContextItems? items = null, IMetricLogger? metricLogger = null,
        IRequestLogger? requestLogger = null, CancellationToken? cancellationToken = null) {

        var context = new HttpRequestContext(
            _httpContext,
            requestData ?? RequestData.Clone(),
            responseData ?? ResponseData.Clone(),
            metricLogger ?? MetricLogger.Clone(),
            requestLogger ?? RequestLogger,
            RequestServices
        ) {
            _serviceProvider = serviceProvider ?? _serviceProvider,
            InvokeParameters = InvokeParameters?.Clone(),
            _cancellationToken = cancellationToken ?? _cancellationToken,
            _items = items ?? _items,
        };

        return context;
    }
}
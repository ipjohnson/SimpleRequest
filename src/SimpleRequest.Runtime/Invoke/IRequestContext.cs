using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke;

public interface IRequestContextItems : IEnumerable<KeyValuePair<object,object?>> {
    IEnumerable<object> Keys { get; }
    
    int Count { get; }
    
    object? Get(object key);
    
    void Set(object key, object value);

    IRequestContextItems Clone();
}

public interface IRequestContext {
    IServiceProvider ServiceProvider { get; }
    
    IRequestHandlerInfo? RequestHandlerInfo { get; set; }
    
    IInvokeParameters? InvokeParameters { get; set; }
    
    IRequestData RequestData { get; }
    
    IResponseData ResponseData { get; }

    IMetricLogger MetricLogger { get; }
    
    IRequestLogger RequestLogger { get; }
    
    DataServices RequestServices { get; }

    CancellationToken CancellationToken { get; }
    
    IRequestContextItems Items { get; } 
    
    IRequestContext Clone(IServiceProvider? serviceProvider = null);
    
}
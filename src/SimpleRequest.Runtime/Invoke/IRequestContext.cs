using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke;

public interface IRequestContext {
    IServiceProvider ServiceProvider { get; }
    
    IRequestHandlerInfo? RequestHandlerInfo { get; set; }
    
    IInvokeParameters? InvokeParameters { get; set; }
    
    IRequestData RequestData { get; }
    
    IResponseData ResponseData { get; }

    IMetricLogger MetricLogger { get; }
    
    IRequestLogger RequestLogger { get; }
    
    RequestServices RequestServices { get; }

    CancellationToken CancellationToken { get; }
    
    IRequestContext Clone(IServiceProvider? serviceProvider = null);
}
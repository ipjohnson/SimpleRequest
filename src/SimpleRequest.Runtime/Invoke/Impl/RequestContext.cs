using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke.Impl;

public class RequestData : IRequestData {
    public RequestData(string path, string method, Stream? body, string contentType, IPathTokenCollection pathTokenCollection, IDictionary<string, StringValues> headers) {
        Path = path;
        Method = method;
        Body = body;
        ContentType = contentType;
        PathTokenCollection = pathTokenCollection;
        Headers = headers;
        StartTime = MachineTimestamp.Now;
    }

    public MachineTimestamp StartTime {
        get;
    }

    public string Path {
        get;
    }

    public string Method {
        get;
    }

    public Stream? Body {
        get;
        set;
    }

    public string ContentType {
        get;
    }

    public IDictionary<string, StringValues> Headers {
        get;
    }

    public IPathTokenCollection PathTokenCollection {
        get;
        set;
    }

    public IRequestData Clone() {
        return new RequestData(Path, Method, Body, ContentType, PathTokenCollection, Headers);
    }
}

public class ResponseData : IResponseData {
    public ResponseData(IDictionary<string,StringValues> headerCollection) {
        Headers = headerCollection;
    }

    public string? ContentType {
        get;
        set;
    }

    public object? ResponseValue {
        get;
        set;
    }

    public string? TemplateName {
        get;
        set;
    }

    public int? Status {
        get;
        set;
    }

    public bool? ShouldCompress {
        get;
        set;
    }

    public Stream? Body {
        get;
        set;
    }

    public Exception? ExceptionValue {
        get;
        set;
    }

    public bool ResponseStarted => (Body?.Position ?? 1) > 0;

    public bool IsBinary {
        get;
        set;
    }

    public bool? ShouldSerialize {
        get;
        set;
    }

    public IDictionary<string,StringValues> Headers {
        get;
    }

    public IResponseData Clone() {
        return new ResponseData(Headers) {
            Body = Body,
            ContentType = ContentType,
            ExceptionValue = ExceptionValue,
            IsBinary = IsBinary,
            Status = Status,
            ShouldSerialize = ShouldSerialize,
            TemplateName = TemplateName
        };
    }
}

public class RequestContext(IServiceProvider serviceProvider,
    IRequestData requestData,
    IResponseData responseData,
    IMetricLogger metricLogger,
    RequestServices requestServices,
    CancellationToken cancellationToken,
    IRequestLogger requestLogger)
    : IRequestContext {

    public IServiceProvider ServiceProvider {
        get;
    } = serviceProvider;

    public IRequestHandlerInfo? RequestHandlerInfo {
        get;
        set;
    }

    public IInvokeParameters? InvokeParameters {
        get;
        set;
    }

    public IRequestData RequestData {
        get;
    } = requestData;

    public IResponseData ResponseData {
        get;
    } = responseData;

    public IMetricLogger MetricLogger {
        get;
    } = metricLogger;

    public IRequestLogger RequestLogger {
        get;
    } = requestLogger;

    public RequestServices RequestServices {
        get;
    } = requestServices;

    public CancellationToken CancellationToken {
        get;
    } = cancellationToken;

    public IRequestContext Clone(IServiceProvider? serviceProvider = null) {
        return new RequestContext(
            serviceProvider ?? ServiceProvider, 
            RequestData.Clone(),
            ResponseData.Clone(),
            MetricLogger.Clone(),
            RequestServices, 
            CancellationToken,
            RequestLogger) {
            RequestHandlerInfo = RequestHandlerInfo,
            InvokeParameters = InvokeParameters?.Clone(),
        };
    }
}
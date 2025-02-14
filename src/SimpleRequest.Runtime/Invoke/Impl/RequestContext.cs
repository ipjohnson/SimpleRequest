using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke.Impl;

public class RequestData : IRequestData {
    public RequestData(string path, string method, Stream? body, string contentType, IPathTokenCollection pathTokenCollection) {
        Path = path;
        Method = method;
        Body = body;
        ContentType = contentType;
        PathTokenCollection = pathTokenCollection;
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

    public IPathTokenCollection PathTokenCollection {
        get;
    }

    public IRequestData Clone() {
        return new RequestData(Path, Method, Body, ContentType, PathTokenCollection);
    }
}

public class ResponseData : IResponseData {

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

    public IResponseData Clone() {
        return new ResponseData {
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
    IContentSerializerManager contentSerializerManager,
    CancellationToken cancellationToken)
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

    public IContentSerializerManager ContentSerializerManager {
        get;
    } = contentSerializerManager;

    public CancellationToken CancellationToken {
        get;
    } = cancellationToken;

    public IRequestContext Clone() {
        return new RequestContext(
            ServiceProvider, 
            RequestData.Clone(),
            ResponseData.Clone(), 
            MetricLogger,
            ContentSerializerManager, 
            CancellationToken) {
            RequestHandlerInfo = RequestHandlerInfo,
            InvokeParameters = InvokeParameters?.Clone(),
        };
    }
}
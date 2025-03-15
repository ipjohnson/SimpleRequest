using System.Collections;
using System.Collections.Immutable;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke.Impl;

public class RequestData : IRequestData {
    public RequestData(
        string path, 
        string method,
        Stream? body, 
        string contentType,
        IPathTokenCollection pathTokenCollection, 
        IDictionary<string, StringValues> headers, 
        IQueryParametersCollection queryParameters, 
        IRequestCookies cookies) {
        Path = path;
        Method = method;
        Body = body;
        ContentType = contentType;
        PathTokenCollection = pathTokenCollection;
        Headers = headers;
        QueryParameters = queryParameters;
        Cookies = cookies;
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

    public IQueryParametersCollection QueryParameters {
        get;
    }

    public IRequestCookies Cookies {
        get;
    }

    public IPathTokenCollection PathTokenCollection {
        get;
        set;
    }

    public IRequestData Clone() {
        return new RequestData(Path, Method, Body, ContentType, PathTokenCollection, Headers, QueryParameters, Cookies);
    }
}

public class ResponseData : IResponseData {
    public ResponseData(IDictionary<string,StringValues> headerCollection, IResponseCookies cookies) {
        Headers = headerCollection;
        Cookies = cookies;
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

    public IResponseCookies Cookies {
        get;
    }

    public IResponseData Clone() {
        return new ResponseData(Headers, Cookies) {
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

internal class RequestContextItem(ImmutableDictionary<object,object?> immutableDictionary) : IRequestContextItems {
    private ImmutableDictionary<object, object?> _immutableDictionary = immutableDictionary;
    
    public IEnumerator<KeyValuePair<object, object?>> GetEnumerator() {
        return _immutableDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerable<object> Keys => _immutableDictionary.Keys;

    public int Count => _immutableDictionary.Count;

    public object? Get(object key) => _immutableDictionary.GetValueOrDefault(key);
    public void Set(object key, object value) {
        _immutableDictionary = _immutableDictionary.SetItem(key, value);
    }

    public IRequestContextItems Clone() {
        return new RequestContextItem(_immutableDictionary);
    }
}

public class RequestContext(IServiceProvider serviceProvider,
    IRequestData requestData,
    IResponseData responseData,
    IMetricLogger metricLogger,
    DataServices requestServices,
    CancellationToken cancellationToken,
    IRequestLogger requestLogger,
    IRequestContextItems? requestContextItem = null)
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


    public DataServices RequestServices {
        get;
    } = requestServices;

    public CancellationToken CancellationToken {
        get;
    } = cancellationToken;

    public IRequestContextItems Items { get; } =
        requestContextItem ?? new RequestContextItem(ImmutableDictionary<object, object?>.Empty);

    public IRequestContext Clone(IServiceProvider? serviceProvider = null) {
        return new RequestContext(
            serviceProvider ?? ServiceProvider, 
            RequestData.Clone(),
            ResponseData.Clone(),
            MetricLogger.Clone(),
            RequestServices, 
            CancellationToken,
            RequestLogger,
            Items.Clone()) {
            RequestHandlerInfo = RequestHandlerInfo,
            InvokeParameters = InvokeParameters?.Clone(),
        };
    }
}
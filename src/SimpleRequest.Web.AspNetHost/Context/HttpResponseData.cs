using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpResponseData : IResponseData {
    private readonly HttpResponse _response;
    private Stream? _body;

    public HttpResponseData(HttpResponse response, Stream? body = null) {
        _response = response;
        _body = body ?? _response.Body;
    }
    public string? ContentType {
        get => _response.ContentType;
        set => _response.ContentType = value;
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
        get => _response.StatusCode;
        set => _response.StatusCode = value ?? 200;
    }

    public bool? ShouldCompress {
        get;
        set;
    }

    public Stream? Body {
        get => _body;
        set => _body = value;
    }

    public Exception? ExceptionValue {
        get;
        set;
    }

    public bool ResponseStarted {
        get => _response.HasStarted;
    }

    public bool IsBinary {
        get;
        set;
    }

    public bool? ShouldSerialize {
        get;
        set;
    }

    public IDictionary<string,StringValues> Headers => _response.Headers;

    public IResponseData Clone() {
        return new HttpResponseData(_response, _body);
    }
}
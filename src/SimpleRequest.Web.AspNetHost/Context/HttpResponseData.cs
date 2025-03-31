using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Invoke;
using CookieOptions = SimpleRequest.Runtime.Cookies.CookieOptions;
using IResponseCookies = SimpleRequest.Runtime.Cookies.IResponseCookies;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpResponseCookies(HttpResponse response) : IResponseCookies {

    public void Append(string key, string value) {
        response.Cookies.Append(key, value);
    }

    public void Append(string key, string value, CookieOptions options) {
        response.Cookies.Append(key, value, new Microsoft.AspNetCore.Http.CookieOptions {
            Domain = options.Domain,
            Path = options.Path,
            Expires = options.Expires,
            HttpOnly = options.HttpOnly,
            Secure = options.Secure,
            SameSite = (SameSiteMode)options.SameSite,
            MaxAge = options.MaxAge,
        });
    }

    public void Delete(string key) {
        response.Cookies.Delete(key);
    }
}

public class HttpResponseData : IResponseData {
    private readonly HttpResponse _response;
    private IDictionary<string, StringValues> _headers;
    private Stream? _body;

    public HttpResponseData(HttpResponse response, Stream? body = null) {
        _response = response;
        _body = body ?? _response.Body;
        _headers = _response.Headers;
        Cookies = new HttpResponseCookies(_response);
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
        set => Noop();
    }

    private void Noop() { }
    
    public bool IsBinary {
        get;
        set;
    }

    public bool? ShouldSerialize {
        get;
        set;
    }

    public IDictionary<string,StringValues> Headers => _headers;

    public IResponseCookies Cookies {
        get; private set;
    }

    public IResponseData Clone(IDictionary<string, StringValues>? headers = null, IResponseCookies? cookies = null) {
        return new HttpResponseData(_response, _body) {
            _headers = headers ?? Headers,
            Cookies = cookies ?? Cookies,
        };
    }
}
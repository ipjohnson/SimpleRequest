using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpQueryParameters : IQueryParametersCollection {
    private readonly IQueryCollection _query;

    public HttpQueryParameters(IQueryCollection query) {
        _query = query;
    }

    public IEnumerable<string> Keys => _query.Keys;

    public int Count => _query.Count;

    public bool ContainsKey(string key) => _query.ContainsKey(key);

    public string this[string key] => _query[key].ToString();

    public bool TryGetValue(string key, out string value) {
        if (_query.TryGetValue(key, out var stringValues)) {
            value = stringValues.ToString();
            return true;
        }
        value = string.Empty;
        return false;
    }
}

public class HttpRequestCookies(HttpRequest request) : IRequestCookies {
    private readonly IRequestCookieCollection _cookies = request.Cookies;

    public IEnumerable<string> Keys => _cookies.Keys;

    public bool ContainsKey(string key) => _cookies.ContainsKey(key);

    public bool TryGetValue(string key, out string? value) {
        if (_cookies.TryGetValue(key, out var cookieValue)) {
            value = cookieValue;
            return true;
        }
        value = null;
        return false;
    }
}

public class HttpRequestData : IRequestData {
    private HttpRequest _request;
    private Stream? _body;
    private MachineTimestamp _timestamp = MachineTimestamp.Now;

    public HttpRequestData(HttpRequest request, Stream? body = null, IPathTokenCollection? pathTokens = null) {
        _request = request;
        _body = body ?? request.Body;
        PathTokenCollection = pathTokens ?? new HttpPathToken(_request.RouteValues);
        QueryParameters = new HttpQueryParameters(_request.Query);
        Cookies = new HttpRequestCookies(_request);
    }

    public MachineTimestamp StartTime => _timestamp;

    public string Path => _request.Path;

    public string Method => _request.Method;

    public Stream? Body {
        get => _body;
        set => _body = value;
    }

    public string ContentType=> _request.ContentType ?? string.Empty;

    public IDictionary<string, StringValues> Headers => _request.Headers;

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
        return new HttpRequestData(_request, _body, PathTokenCollection);
    }
}
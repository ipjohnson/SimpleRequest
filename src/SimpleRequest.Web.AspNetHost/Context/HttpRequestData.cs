using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Web.AspNetHost.Context;

public class HttpRequestData : IRequestData {
    private HttpRequest _request;
    private Stream? _body;
    private MachineTimestamp _timestamp = MachineTimestamp.Now;

    public HttpRequestData(HttpRequest request, Stream? body = null, IPathTokenCollection? pathTokens = null) {
        _request = request;
        _body = body ?? request.Body;
        PathTokenCollection = pathTokens ?? new HttpPathToken(_request.RouteValues);
    }

    public MachineTimestamp StartTime => _timestamp;

    public string Path => _request.Path;

    public string Method => _request.Method;

    public Stream? Body {
        get => _body;
        set => _body = value;
    }

    public string ContentType=> _request.ContentType ?? string.Empty;

    public IPathTokenCollection PathTokenCollection {
        get;
    }

    public IRequestData Clone() {
        return new HttpRequestData(_request, _body, PathTokenCollection);
    }
}
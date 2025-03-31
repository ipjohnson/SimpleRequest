using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Runtime.Invoke;

public interface IRequestData {
    MachineTimestamp StartTime { get; }
    
    string Path { get; }
    
    string Method { get; }
    
    Stream? Body { get; set; }

    string ContentType {
        get;
    }

    IDictionary<string,StringValues> Headers { get; }
    
    IQueryParametersCollection QueryParameters { get; }
    
    IRequestCookies Cookies { get; }
    
    IPathTokenCollection PathTokenCollection { get; }
    
    IRequestData Clone(
        string? path = null, 
        string? method = null, 
        string? contentType = null,
        IDictionary<string,StringValues>? headers = null,
        IQueryParametersCollection? queryParameters = null,
        IRequestCookies? cookies = null,
        IPathTokenCollection? pathTokenCollection = null);
}
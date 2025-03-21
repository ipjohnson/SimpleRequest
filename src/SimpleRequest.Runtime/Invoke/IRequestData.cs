using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke.Impl;
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
    
    IRequestData Clone();
}
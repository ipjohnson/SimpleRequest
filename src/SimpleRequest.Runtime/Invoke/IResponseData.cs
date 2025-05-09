using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;

namespace SimpleRequest.Runtime.Invoke;

public interface IResponseData {
    
    string? ContentType { get; set; }

    object? ResponseValue { get; set; }

    string? TemplateName { get; set; }

    int? Status { get; set; }

    bool? ShouldCompress { get; set; }

    Stream? Body { get; set; }

    Exception? ExceptionValue { get; set; }

    bool ResponseStarted { get; set; }

    bool IsBinary { get; set; }

    bool? ShouldSerialize { get; set; }
    
    IDictionary<string,StringValues> Headers { get; }
    
    IResponseCookies Cookies { get; }
    
    IResponseData Clone(IDictionary<string,StringValues>? headers = null, IResponseCookies? cookies = null);
}
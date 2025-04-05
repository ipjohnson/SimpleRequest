using System.IO.Compression;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Testing;

public record RequestModel(
    string Path,
    string Method,
    Stream Body
    );

public interface IResponseModel {
    int StatusCode {
        get;
    }

    Stream Body {
        get;
    }

    string ContentType {
        get;
    }

    IDictionary<string, StringValues> Headers {
        get;
    }

    IResponseCookies Cookies {
        get;
    }

    void AssertOk() {
        if (StatusCode is < 200 or >= 300) {
            throw new Exception("Status code is invalid: " + StatusCode);
        }
    }

    Task<T> Get<T>();
}

public class ResponseModel(
    IStreamCompressionService streamCompressionService,
    IServiceProvider serviceProvider,
    IResponseData responseData,
    IContentSerializerManager contentSerializerManager
) : IResponseModel {
    public int StatusCode => responseData.Status ?? 200;
    
    public Stream Body => responseData.Body!;

    public string ContentType => responseData.ContentType ?? "";
    
    public IDictionary<string, StringValues> Headers => responseData.Headers;
    
    public IResponseCookies Cookies => responseData.Cookies;

    public async Task<T> Get<T>() {
        var serializer = contentSerializerManager.GetSerializer(responseData.ContentType ?? "application/json");

        if (serializer is null) {
            throw new Exception($"No serializer for {responseData.ContentType}");
        }

        if (responseData.Body == null) {
            throw new Exception($"No response body");
        }
        
        var body = responseData.Body;
        body.Position = 0;

        if (responseData.Headers.TryGetValue("Content-Encoding", out var value)) {
            body = 
                streamCompressionService.GetStream(body, value.ToString(), CompressionMode.Decompress) ?? body;
        }

        return await DeserializeBody<T>(serializer, body);
    }

    protected virtual async Task<T> DeserializeBody<T>(IContentSerializer serializer, Stream body) {
        return (await serializer.DeserializeAsync<T>(body))!;
    }
}
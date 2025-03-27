using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;
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

public class ResponseModel(
    IStreamCompressionService streamCompressionService,
    IServiceProvider serviceProvider,
    IResponseData responseData,
    IContentSerializerManager contentSerializerManager
) {
    public int StatusCode => responseData.Status ?? 200;
    
    public Stream Body => responseData.Body!;

    public string ContentType => responseData.ContentType ?? "";
    
    public IDictionary<string, StringValues> Headers => responseData.Headers;
    
    public IResponseCookies Cookies => responseData.Cookies;
    
    public void AssertOk() {
        if (responseData.Status is < 200 or >= 300) {
            throw new Exception("Status code is invalid: " + responseData.Status);
        }
    }

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

        if (typeof(T) == typeof(string) && 
            serviceProvider.GetRequiredService<RequestResponseConfiguration>().DefaultStingContentType == "text/plain") {
            return (T)(object)await new StreamReader(body).ReadToEndAsync();
        }
        
        return await serializer.Deserialize<T>(body) ?? throw new Exception($"Response could not be deserialized");
    }
}
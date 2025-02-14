using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Testing;

public record RequestModel(
    string Path,
    string Method,
    Stream Body
    );

public class ResponseModel(
    IResponseData responseData,
    IContentSerializerManager contentSerializerManager
) {
    public int StatusCode => responseData.Status ?? 200;
    
    public Stream Body => responseData.Body!;
    
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
        
        return await serializer.Deserialize<T>(responseData.Body) ?? throw new Exception($"Response could not be deserialized");
    }
}
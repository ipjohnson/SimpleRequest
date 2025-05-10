using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Testing.JsonRpc;

public class JsonRpcRequest<T>(string method, T @params, string id) {
    public string Jsonrpc => "2.0";
    
    public string Method {
        get;
    } = method;

    public T Params {
        get;
    } = @params;

    public string Id {
        get;
    } = id;
}

public record JsonRpcResponse<T>(T Result, string Id);

public record JsonRpcError(int Code, string Message);

public record JsonRpcError<T>(int Code, string Message, T? Data) : JsonRpcError(Code, Message);

public class JsonRpcResponseModel(
    IStreamCompressionService streamCompressionService,
    IServiceProvider serviceProvider, 
    IResponseData responseData,
    IContentSerializerManager contentSerializerManager) :
    ResponseModel(streamCompressionService, serviceProvider, responseData, contentSerializerManager), IResponseModel {

    protected override async Task<T> DeserializeBody<T>(IContentSerializer serializer, Stream body) {
        if (typeof(T).IsAssignableTo(typeof(JsonRpcError)))
        {
            return await base.DeserializeBody<T>(serializer, body);
        }
        
        var response = await base.DeserializeBody<JsonRpcResponse<T>>(serializer, body);
        
        return response.Result;
    }
}
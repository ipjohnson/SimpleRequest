using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Testing.JsonRpc;

public class JsonRpcResponseModel(
    IStreamCompressionService streamCompressionService,
    IServiceProvider serviceProvider, 
    IResponseData responseData,
    IContentSerializerManager contentSerializerManager) : ResponseModel(streamCompressionService, serviceProvider, responseData, contentSerializerManager) {
    
}
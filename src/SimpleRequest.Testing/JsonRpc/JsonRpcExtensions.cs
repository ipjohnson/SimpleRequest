using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Testing.JsonRpc;

public static class JsonRpcExtensions {
    public static async Task<IResponseModel> JsonRpc(this RequestHarness harness, string path, string method, object parameters) {
        var response = await harness.Post(path, new JsonRpcRequest<object>(method, parameters, "1"));
        
        return new JsonRpcResponseModel(
            harness.ServiceProvider.GetRequiredService<IStreamCompressionService>(),
            harness.ServiceProvider,
            response.ResponseData,
            harness.ServiceProvider.GetRequiredService<IContentSerializerManager>()
        );
    }
}
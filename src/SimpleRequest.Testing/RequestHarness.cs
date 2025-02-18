using CompiledTemplateEngine.Runtime.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Testing;

public class RequestHarness(IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    IMemoryStreamPool memoryStreamPool,
    IContentSerializerManager contentSerializer) {
    
    public IServiceProvider ServiceProvider => serviceProvider;
    
    public async Task<ResponseModel> Invoke(string method, string path, object? payload = null) {
        using var requestStreamReservation = memoryStreamPool.Get();
        using var responseStreamReservation = memoryStreamPool.Get();
        await using var scope = serviceProvider.CreateAsyncScope();

        if (payload != null) {
            var serializer = contentSerializer.GetSerializer("application/json");

            if (serializer == null) {
                throw new Exception("Could not find content serializer for " + method);
            }
            
            await serializer.Serialize(requestStreamReservation.Item, payload);
            
            requestStreamReservation.Item.Position = 0;
        }

        var requestData = new RequestData(
            path,
            method,
            requestStreamReservation.Item,
            "application/json",
            new PathTokenCollection());
        
        var responseData = new ResponseData {
            Body = responseStreamReservation.Item,
        };
        
        var context = new RequestContext(
            scope.ServiceProvider,
            requestData, 
            responseData,
            new NullMetricsLogger(),
            contentSerializer,
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>());
        
        await requestInvocationEngine.Invoke(context);

        responseStreamReservation.Item.Position = 0;
        var memoryStream = new MemoryStream();
        
        await responseStreamReservation.Item.CopyToAsync(memoryStream);
        
        memoryStream.Position = 0;
        var response = context.ResponseData.Clone();
        response.Body = memoryStream;
        
        return new ResponseModel(response, contentSerializer);
    }
}

public static class RequestHarnessExtensions {
    public static Task<ResponseModel> Post(this RequestHarness harness, string path, object? payload = null) => 
        harness.Invoke("POST", path, payload);
   
    public static Task<ResponseModel> Put(this RequestHarness harness, string path, object? payload = null) => 
        harness.Invoke("PUT", path, payload);
    
    public static Task<ResponseModel> Patch(this RequestHarness harness, string path, object? payload = null) => 
        harness.Invoke("PATCH", path, payload);
    
    public static Task<ResponseModel> Get(this RequestHarness harness, string path) => 
        harness.Invoke("GET", path);
    
    public static Task<ResponseModel> Delete(this RequestHarness harness, string path, object? payload = null) => 
        harness.Invoke("DELETE", path, payload);
}
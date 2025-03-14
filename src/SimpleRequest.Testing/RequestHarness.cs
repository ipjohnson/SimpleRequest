using DependencyModules.xUnit.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Pools;
using SimpleRequest.Testing.Interfaces;

namespace SimpleRequest.Testing;

public class RequestHarness(IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    IMemoryStreamPool memoryStreamPool,
    DataServices requestServices,
    IEnumerable<IRequestDataEnrichment> enrichRequestData,
    IEnumerable<IResponseDataValidator> validateResponseData) {
    private readonly IReadOnlyList<IRequestDataEnrichment> _enrichRequestData = enrichRequestData.ToList();
    private readonly IReadOnlyList<IResponseDataValidator> _validateResponseData = validateResponseData.ToList();
    
    public IServiceProvider ServiceProvider => serviceProvider;

    public async Task<ResponseModel> Invoke(string method, string path, object? payload = null, List<ValueTuple<string, StringValues>>? headers = null) {
        var headerDictionary =new Dictionary<string, StringValues>();
        if (headers != null) {
            foreach (var tuple in headers) {
                headerDictionary[tuple.Item1] = tuple.Item2;
            }
        }
        
        using var requestStreamReservation = memoryStreamPool.Get();

        requestStreamReservation.Item.SetLength(0);
        
        if (payload != null) {
            var serializer = requestServices.ContentSerializerManager.GetSerializer("application/json");

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
            new PathTokenCollection(),
            headerDictionary);
        
        return await Invoke(requestData);
    }
    
    public async Task<ResponseModel> Invoke(IRequestData request) {        
        using var responseStreamReservation = memoryStreamPool.Get();
        await using var scope = serviceProvider.CreateAsyncScope();
        var testCaseInfo = scope.ServiceProvider.GetRequiredService<ITestCaseInfo>();
        request = ApplyEnrichments(scope.ServiceProvider, testCaseInfo, request);

        var responseData = new ResponseData(new Dictionary<string, StringValues>()) {
            Body = responseStreamReservation.Item,
        };
        
        var context = new RequestContext(
            scope.ServiceProvider,
            request,
            responseData,
            new NullMetricsLogger(),
            requestServices,
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>());
        
        await requestInvocationEngine.Invoke(context);

        responseStreamReservation.Item.Position = 0;
        var memoryStream = new MemoryStream();
        
        await responseStreamReservation.Item.CopyToAsync(memoryStream);
        
        memoryStream.Position = 0;
        var response = context.ResponseData.Clone();
        response.Body = memoryStream;
        
        var responseModel = new ResponseModel(response, requestServices.ContentSerializerManager);
        
        await ValidateResponse(scope.ServiceProvider, testCaseInfo, responseModel);
        
        return responseModel;
    }

    private async Task ValidateResponse(IServiceProvider scopeServiceProvider, ITestCaseInfo testCaseInfo, ResponseModel responseModel) {
        foreach (var validate in validateResponseData) {
            await validate.ValidateResponse(scopeServiceProvider, testCaseInfo, responseModel);
        }

        foreach (var attribute in testCaseInfo.TestMethodAttributes) {
            if (attribute is IResponseDataValidator validator) {
                await validator.ValidateResponse(scopeServiceProvider, testCaseInfo, responseModel);
            }
        }
    }

    private IRequestData ApplyEnrichments(IServiceProvider scopedProvide, ITestCaseInfo testCaseInfo, IRequestData request) {
        
        foreach (var enrich in _enrichRequestData) {
            request = enrich.EnrichRequestData(scopedProvide,testCaseInfo, request);
        }

        foreach (var attribute in testCaseInfo.TestMethodAttributes) {
            if (attribute is IRequestDataEnrichment enrichRequestData) {
                request = enrichRequestData.EnrichRequestData(scopedProvide, testCaseInfo, request);
            }
        }
        
        return request;
    }
}

public static class RequestHarnessExtensions {
    public static Task<ResponseModel> Post(this RequestHarness harness, string path, object? payload = null, List<ValueTuple<string, StringValues>>? headers = null) => 
        harness.Invoke("POST", path, payload, headers);
   
    public static Task<ResponseModel> Put(this RequestHarness harness, string path, object? payload = null, List<ValueTuple<string, StringValues>>? headers = null) => 
        harness.Invoke("PUT", path, payload, headers);
    
    public static Task<ResponseModel> Patch(this RequestHarness harness, string path, object? payload = null, List<ValueTuple<string, StringValues>>? headers = null) => 
        harness.Invoke("PATCH", path, payload, headers);
    
    public static Task<ResponseModel> Get(this RequestHarness harness, string path, List<ValueTuple<string, StringValues>>? headers = null) => 
        harness.Invoke("GET", path, null, headers);
    
    public static Task<ResponseModel> Delete(this RequestHarness harness, string path, object? payload = null, List<ValueTuple<string, StringValues>>? headers = null) => 
        harness.Invoke("DELETE", path, payload, headers);
}
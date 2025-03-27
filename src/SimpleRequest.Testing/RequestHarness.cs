using System.IO.Compression;
using DependencyModules.xUnit.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Pools;
using SimpleRequest.Runtime.QueryParameters;
using SimpleRequest.Testing.Interfaces;

namespace SimpleRequest.Testing;

public class RequestHarness(
    IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    IMemoryStreamPool memoryStreamPool,
    DataServices requestServices,
    IStreamCompressionService streamCompressionService,
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
        IAsyncDisposable? compressedRequestStream = null;
        
        using var requestStreamReservation = memoryStreamPool.Get();

        requestStreamReservation.Item.SetLength(0);
        
        if (!headerDictionary.TryGetValue("Content-Type", out var contentType)) {
            contentType = "application/json";
        }
        
        if (payload != null) {
            var serializer = requestServices.ContentSerializerManager.GetSerializer(contentType);

            if (serializer == null) {
                throw new Exception("Could not find content serializer for " + method);
            }

            var body = requestStreamReservation.Item as Stream;
            if (headerDictionary.TryGetValue("Content-Encoding", out var encoding)) {
                var stream = streamCompressionService.GetStream(body, encoding.ToString(), CompressionMode.Compress);
                
                if (stream != null) {
                    body = stream;
                    compressedRequestStream = body as IAsyncDisposable;
                }
            }
            
            await serializer.Serialize(requestStreamReservation.Item, payload);
            
            requestStreamReservation.Item.Position = 0;
        }

        var (newPath, queryParams) = ParsePath(path);
        
        var requestData = new RequestData(
            newPath,
            method,
            requestStreamReservation.Item,
            contentType.ToString(),
            new PathTokenCollection(),
            headerDictionary,
            queryParams,
            new RequestCookies());
        
        var response = await Invoke(requestData);

        if (compressedRequestStream != null) {
            await compressedRequestStream.DisposeAsync();
        }
        
        return response;
    }

    private (string newPath, IQueryParametersCollection queryParams) ParsePath(string path) {
        var splitPath = path.Split('?');
        var queryParams = new Dictionary<string, string>();

        if (splitPath.Length > 1) {
            var query = splitPath[1];
            var queryParts = query.Split('&');
            foreach (var queryPart in queryParts) {
                var queryParam = queryPart.Split('=');
                queryParams[queryParam[0]] = queryParam[1];
            }
        }
        
        return (splitPath[0], new QueryParametersCollection(queryParams));
    }

    public async Task<ResponseModel> Invoke(IRequestData request) {        
        using var responseStreamReservation = memoryStreamPool.Get();
        await using var scope = serviceProvider.CreateAsyncScope();
        var testCaseInfo = scope.ServiceProvider.GetRequiredService<ITestCaseInfo>();
        request = ApplyEnrichments(scope.ServiceProvider, testCaseInfo, request);

        var responseCookies = new ResponseCookies();
        var responseData = new ResponseData(new Dictionary<string, StringValues>(), responseCookies) {
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
        
        var responseModel = new ResponseModel(
            streamCompressionService, 
            serviceProvider,
            response,
            requestServices.ContentSerializerManager);
        
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
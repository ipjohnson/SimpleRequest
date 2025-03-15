using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Runtime.Tests.Models;

public class TestRequestContext {
    public static RequestContext GenerateTestContext(IServiceProvider serviceProvider) {
        return new RequestContext(
            serviceProvider,
            new RequestData(
                "/test", 
                "GET",
                new MemoryStream(), 
                "application/json",
                new PathTokenCollection(),
                new Dictionary<string, StringValues>(),
                new QueryParametersCollection(new Dictionary<string, string>()),
                new RequestCookies()),
            new ResponseData(
                new Dictionary<string, StringValues>(),
                new ResponseCookies()) { Body = new MemoryStream()},
            new NullMetricsLogger(),
            serviceProvider.GetRequiredService<DataServices>(),
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>()
        );
    }
}
    
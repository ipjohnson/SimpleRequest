using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

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
                new Dictionary<string, StringValues>()),
            new ResponseData(new Dictionary<string, StringValues>()) { Body = new MemoryStream()},
            new NullMetricsLogger(),
            serviceProvider.GetRequiredService<RequestServices>(),
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>()
        );
    }
}
    
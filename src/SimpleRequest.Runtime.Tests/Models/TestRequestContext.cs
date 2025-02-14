using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Tests.Models;

public class TestRequestContext {
    public static RequestContext GenerateTestContext(IServiceProvider serviceProvider) {
        return new RequestContext(
            serviceProvider,
            new RequestData("/test", "GET", new MemoryStream(), "application/json", new PathTokenCollection()),
            new ResponseData() { Body = new MemoryStream()},
            new NullMetricsLogger(),
            serviceProvider.GetRequiredService<IContentSerializerManager>(),
            CancellationToken.None
        );
    }
}
    
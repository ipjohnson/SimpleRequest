using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using Xunit;

namespace TestApp.WebHandlers.Tests.Handlers.Compression;

public class LargeResponseHandlerTests {
    [ModuleTest]
    public async Task ReturnCompressedResponse(RequestHarness harness) {
        var response = await harness.Get("/large-response", 
            [("Accept-Encoding", "gzip"), ("Content-Type", "text/plain")]);
        
        response.AssertOk();
        Assert.Equal("gzip", response.Headers["Content-Encoding"]);
        
        var content = await response.Get<string>();
        var splitString = content.Split('\n');
        Assert.Equal(11, splitString.Length);
    }
}
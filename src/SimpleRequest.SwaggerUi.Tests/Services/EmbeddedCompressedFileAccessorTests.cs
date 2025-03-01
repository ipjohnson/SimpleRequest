using DependencyModules.xUnit.Attributes;
using SimpleRequest.SwaggerUi.Services;
using Xunit;

namespace SimpleRequest.SwaggerUi.Tests.Services;

public class EmbeddedCompressedFileAccessorTests {
    [ModuleTest]
    public async Task GetIndexHtml(EmbeddedCompressedFileAccessor accessor) {
        var bytes = await accessor.ReadFile("index.html");
        
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }
    
    [ModuleTest]
    public async Task GetIndexCss(EmbeddedCompressedFileAccessor accessor) {
        var bytes = await accessor.ReadFile("index.css");
        
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }
}
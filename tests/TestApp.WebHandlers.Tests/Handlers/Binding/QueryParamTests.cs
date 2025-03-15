using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using Xunit;

namespace TestApp.WebHandlers.Tests.Handlers.Binding;

public class QueryParamTests {
    [ModuleTest]
    public async Task BindQueryParam(RequestHarness harness) {
        var response = await harness.Get("/Binding/GetQueryParam?query-param=test");
        
        response.AssertOk();
        
        var value = await response.Get<string>();
        Assert.Equal("test", value);
    }
}
using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using Xunit;

namespace TestApp.WebHandlers.Tests.Handlers.RouteHandlers;

public class RouteTestHandlerTests {
    [ModuleTest]
    public async Task CatchAllRouteTest(RequestHarness harness) {
        var response = await harness.Get("/routing-test/some-value");
        
        response.AssertOk();

        var value = await response.Get<string>();

        Assert.Equal("some-value", value);
    }

    [ModuleTest]
    public async Task NestedRouteTest(RequestHarness harness) {
        var response = await harness.Get("/routing-test/route-value/nested-value");
              
        response.AssertOk();

        var value = await response.Get<string>();

        Assert.Equal("route-value/nested-value", value);  
        
        harness.Invoke("", "", null, [("", "")]);
    }
}
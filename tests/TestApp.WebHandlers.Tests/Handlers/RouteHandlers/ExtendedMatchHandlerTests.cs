using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using Xunit;

namespace TestApp.WebHandlers.Tests.Handlers.RouteHandlers;

public class ExtendedMatchHandlerTests {
    [ModuleTest]
    public async Task ValidateHeaderMatch(RequestHarness harness) {
        var response = await harness.Get("/extended-match");
        
        response.AssertOk();
        var value = await response.Get<string>();
        Assert.Equal("NoHeader", value);
        
        response = await harness.Get("/extended-match", [("Test-Value", "header-value")]);
        response.AssertOk();
        value = await response.Get<string>();
        Assert.Equal("header-value", value);
    }
}
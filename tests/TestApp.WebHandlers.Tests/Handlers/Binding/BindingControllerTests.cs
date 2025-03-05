using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using SimpleRequest.Testing.Attributes;
using Xunit;

namespace TestApp.WebHandlers.Tests.Handlers.Binding;

public class BindingControllerTests {
    [ModuleTest]
    public async Task HeaderBindingTest(RequestHarness harness) {
        var response = await harness.Get("/Binding/GetHeader", [("Test-Value", "header-value")]);
        
        response.AssertOk();

        var value = await response.Get<string>();
        Assert.Equal("header-value", value);
    }


    [ModuleTest]
    [RequestHeader("Test-Value", "header-value")]
    public async Task BodyBindingTest(RequestHarness harness) {
        var response = await harness.Get("/Binding/GetHeader");
        
        response.AssertOk();

        var value = await response.Get<string>();
        Assert.Equal("header-value", value);
    }
}
using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;

namespace TestApp.WebHandlers.Tests.Handlers.StaticContent;

public class StaticContentTests {
    [ModuleTest]
    public async Task Test1(RequestHarness harness) {
        var response = await harness.Get("/static-content/index.html");
        
        response.AssertOk();
        var content = await response.Get<string>();
    }
}
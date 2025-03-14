using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using Xunit;

namespace TestApp.WebHandlers.Tests.Handlers.Templates;

public class TemplateHandlerTests {
    [ModuleTest]
    public async Task InvokeTemplateHandler(RequestHarness harness) {
        var response = await harness.Get("/HelloWorld");
        
        response.AssertOk();

        var document = response.ParseHtml();
        
        Assert.Equal("Hello World", document.Title);
    }
}
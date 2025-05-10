using DependencyModules.Runtime.Helpers;
using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using SimpleRequest.Testing.JsonRpc;
using Xunit;

namespace TestApp.JsonHandlers.Tests.Handlers;

public record CreateWidgetReq(string Name);

public class WidgetHandlersTests {
    [ModuleTest]
    public async Task CreateWidget(RequestHarness harness) {
        var modules = DependencyRegistry<Generated.JsonRpcSharedModule>.GetModules();
        
        var response = await harness.JsonRpc("/json-rpc", "CreateWidget", new CreateWidgetReq("Test Widget"));

        response.AssertOk();
        var responseString = await response.Get<string>();
        
        Assert.Equal("Test Widget created", responseString);
    }


}
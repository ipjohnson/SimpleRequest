using DependencyModules.xUnit.Attributes;
using SimpleRequest.Testing;
using TestApp.FunctionHandlers.Handlers;
using TestApp.FunctionHandlers.Models;

namespace TestApp.FunctionHandlers.Tests.Handlers;

public class SomeHandlerTests {
    [ModuleTest]
    public async Task InvokeGetModel(RequestHarness harness) {
        var id = Guid.NewGuid().ToString();
        
        var response = await harness.Post(
            "get-notes-model", new GetModelRequest(id));
        
        response.AssertOk();

        var value = await response.Get<NoteModel>();
        
        Assert.NotNull(value);
        Assert.Equal(id, value.NoteId);
    }
}
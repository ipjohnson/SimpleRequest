using SimpleRequest.Functions.Runtime.Attributes;

namespace TestApp.FunctionHandlers.Handlers;

public class VoidReturnHandler {

    [Function(Name = "VoidReturnHandler")]
    public Task Handle() {
        int? id = 0;
        var value = (int)id;
        return Task.CompletedTask;
    }
}
using SimpleRequest.Functions.Runtime.Attributes;

namespace TestApp.FunctionHandlers.Handlers;

public class VoidReturnHandler {

    [Function(Name = "VoidReturnHandler")]
    public Task Handle() {
        return Task.CompletedTask;
    }
}
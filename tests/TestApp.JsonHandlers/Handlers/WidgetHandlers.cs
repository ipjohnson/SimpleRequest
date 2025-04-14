using SimpleRequest.JsonRpc;

namespace TestApp.JsonHandlers.Handlers;

public class WidgetHandlers {
    
    [JsonRpcFunction]
    public string CreateWidget(string name) {
        return name + " created";
    }
    
        
    [JsonRpcFunction]
    public async Task<string> CreateWidgetAsync(string name) {
        return name + " created";
    }
}
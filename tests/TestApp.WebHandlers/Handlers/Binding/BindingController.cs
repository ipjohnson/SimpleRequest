using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime.Attributes;

namespace TestApp.WebHandlers.Handlers.Binding;

public class BindingController {
    
    [Get("/Binding/GetHeader")]
    public Task<string> GetHeader([FromHeader("Test-Value")]string header) {
        return Task.FromResult(header);
    }
}
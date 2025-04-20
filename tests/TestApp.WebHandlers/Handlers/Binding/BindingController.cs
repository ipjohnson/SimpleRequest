using SimpleRequest.Models.Attributes;
using SimpleRequest.Runtime.Attributes;

namespace TestApp.WebHandlers.Handlers.Binding;

public class BindingController {
    
    [Get("/Binding/GetHeader")]
    public Task<string> GetHeader([FromHeader("Test-Value")]string header) {
        return Task.FromResult(header);
    }

    [Get("/Binding/GetQueryParam")]
    public string GetQueryParam([FromQuery("query-param")] string query) {
        return query;
    }
}
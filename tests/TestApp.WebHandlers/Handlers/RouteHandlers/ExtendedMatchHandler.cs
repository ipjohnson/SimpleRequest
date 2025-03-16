using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime.Attributes;

namespace TestApp.WebHandlers.Handlers.RouteHandlers;

public class ExtendedMatchHandler {
    [Get("/extended-match")]
    public async Task<string> NoHeader() {
        return "NoHeader";
    }
    
    [Get("/extended-match")]
    [HeaderPresent("Test-Value")]
    public async Task<string> WithHeader([FromHeader("Test-Value")]string header) {
        return header;
    }
}
using SimpleRequest.Models.Attributes;
using SimpleRequest.Runtime.Attributes;
using TestApp.WebHandlers.Models;

namespace TestApp.WebHandlers.Handlers.Compression;

public class LargeResponseHandler {
    
    [Get("/large-response")]
    [ContentType("text/plain")]
    public async IAsyncEnumerable<string> GetStrings() {
        for (int i = 0; i < 10; i++) {
            yield return "Hello World " + i + "\n";
        }
    }

    [Post("/post-values")]
    public async Task<string> PostValues(NoteModel model) {
        
        return model.ToString();
    }
}
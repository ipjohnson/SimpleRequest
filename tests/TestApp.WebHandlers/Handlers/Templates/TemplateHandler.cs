using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime.Attributes;
using TestApp.WebHandlers.Models;

namespace TestApp.WebHandlers.Handlers.Templates;

public class TemplateHandler {
    [Get("/HelloWorld")]
    [Template("HelloWorld")]
    public NoteModel HelloWorld() {
        return new NoteModel(
            "1",
            "Hello World",
            "Hello World Message",
            DateTime.Now, 
            DateTime.Now
        );
    }
}
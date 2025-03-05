using SimpleRequest.Web.Runtime.Attributes;
using TestApp.WebHandlers.Models;

namespace TestApp.WebHandlers.Handlers.Notes;

#pragma warning disable CS1998 
[BasePath("/notes")]
public class NotesHandler {
    [Get]
    public async Task<List<NoteModel>> GetNotes() {
        var notes = new List<NoteModel> {
            new NoteModel(
                "1",
                "Title",
                "Message",
                DateTime.Now.AddHours(-1), 
                DateTime.Now
            )
        };

        return notes;
    }
    
    [Get("/{id}")]
    public async Task<NoteModel> GetNote(string id) {
        return new NoteModel(
            id,
            "Title",
            "Message",
            DateTime.Now.AddHours(-1), 
            DateTime.Now
        );
    }

    [Put("/{id}")]
    public async Task PostNote(NoteModel note) {
        
    }
}
#pragma warning restore CS1998 
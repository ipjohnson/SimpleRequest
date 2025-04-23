using SimpleRequest.Models.Attributes;
using TestApp.WebHandlers.Models;

namespace TestApp.WebHandlers.Interfaces;

[BasePath("/Typed/notes")]
public interface INoteHandler {
    
    [Get("/Typed/notes")]
    Task<List<NoteModel>> GetNotes();
    
    [Get("/Typed/notes/{id}")]
    Task<NoteModel> GetNote(int id);
}

public interface ITest : INoteHandler;

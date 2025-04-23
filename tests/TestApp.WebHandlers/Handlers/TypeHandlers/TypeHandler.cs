using SimpleRequest.Models.Attributes;
using TestApp.WebHandlers.Interfaces;
using TestApp.WebHandlers.Models;

namespace TestApp.WebHandlers.Handlers.TypeHandlers;

[OperationsHandler]
public class TypedNoteHandler : ITest {

    public Task<List<NoteModel>> GetNotes() {
        return Task.FromResult(new List<NoteModel>());
    }

    public Task<NoteModel> GetNote(int id) {
        return Task.FromResult(new NoteModel(
            $"{id}", $"Note: {id}", "Model", DateTime.Now, DateTime.Now));
    }
}
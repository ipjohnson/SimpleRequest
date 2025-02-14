using SimpleRequest.Functions.Runtime.Attributes;
using TestApp.FunctionHandlers.Filters;
using TestApp.FunctionHandlers.Models;

namespace TestApp.FunctionHandlers.Handlers;

public record GetModelRequest(string Id);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
public class SomeHandler {
    [Function(Name = "get-notes-model")]
    [CustomFilter]
    public async Task<NoteModel> GetModel(GetModelRequest request) {
        return new NoteModel(
            request.Id, 
            "Title", 
            "Message", 
            DateTime.Now.AddHours(-1), 
            DateTime.Now);
    }


    [Function(Name = "get-notes-{id}")]
    public async Task<NoteModel> GetNotes(GetModelRequest request) {
        return new NoteModel(
            request.Id, 
            "Title", 
            "Message", 
            DateTime.Now.AddHours(-1), 
            DateTime.Now);
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
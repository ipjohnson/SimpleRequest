namespace TestApp.FunctionHandlers.Models;

public record NoteModel(
    string NoteId,
    string Title,
    string Message,
    DateTime Created,
    DateTime Modified
    );
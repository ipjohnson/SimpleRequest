using DependencyModules.Runtime.Attributes;

namespace TestApp.WebHandlers.Services;

public interface INoteService {
    string NoteText { get; }
}

[SingletonService]
public class NoteService : INoteService {

    public string NoteText {
        get;
    }
}
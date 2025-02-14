namespace SimpleRequest.Runtime.Invoke;

public interface IRequestData {
    string Path { get; }
    
    string Method { get; }
    
    Stream? Body { get; set; }

    string ContentType {
        get;
    }

    IPathTokenCollection PathTokenCollection { get; }
    
    IRequestData Clone();
}
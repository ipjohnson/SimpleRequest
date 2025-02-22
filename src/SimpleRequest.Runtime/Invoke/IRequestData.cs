using SimpleRequest.Runtime.Diagnostics;

namespace SimpleRequest.Runtime.Invoke;

public interface IRequestData {
    MachineTimestamp StartTime { get; }
    
    string Path { get; }
    
    string Method { get; }
    
    Stream? Body { get; set; }

    string ContentType {
        get;
    }

    IPathTokenCollection PathTokenCollection { get; }
    
    IRequestData Clone();
}
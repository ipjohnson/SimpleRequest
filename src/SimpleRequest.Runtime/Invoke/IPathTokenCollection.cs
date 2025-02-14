using Microsoft.Extensions.Primitives;

namespace SimpleRequest.Runtime.Invoke;

public interface IPathTokenCollection {
    int Count { get; }

    object? Get(string key);
    
    void Set(string key, object value);
}
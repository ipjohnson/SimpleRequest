namespace SimpleRequest.Runtime.Invoke.Impl;

public class EmptyPathTokenCollection : IPathTokenCollection {
    public static readonly EmptyPathTokenCollection Instance = new();
    
    public int Count => 0;

    public object? Get(string key) => null;

    public void Set(string key, object value) { }
}

public class PathTokenCollection : IPathTokenCollection {
    private readonly IDictionary<string, object> _values;

    public PathTokenCollection() : this(new Dictionary<string, object>()) { }

    public PathTokenCollection(IDictionary<string, object> values) {
        _values = values;
    }
    
    public int Count => _values.Count;

    public object? Get(string key) {
        if (_values.TryGetValue(key, out var value)) {
            return value;
        }
        return null;
    }

    public void Set(string key, object value) {
        _values[key] = value;
    }
}
namespace SimpleRequest.Runtime.QueryParameters;

public interface IQueryParametersCollection {
    IEnumerable<string> Keys { get; }
    
    int Count { get; }
    
    bool ContainsKey(string key);
    
    string this[string key] { get; }
    
    bool TryGetValue(string key, out string value);

    string? GetValueOrDefault(string key, string? defaultValue = null) {
        return TryGetValue(key, out var value) ? value : defaultValue;
    }
}

public class QueryParametersCollection(IDictionary<string,string> parameters) : IQueryParametersCollection {

    public IEnumerable<string> Keys => parameters.Keys;

    public int Count => parameters.Count;

    public bool ContainsKey(string key) => parameters.ContainsKey(key);

    public string this[string key] => parameters[key];

    public bool TryGetValue(string key, out string value) => parameters.TryGetValue(key, out value!);
}
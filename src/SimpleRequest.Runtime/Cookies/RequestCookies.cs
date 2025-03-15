namespace SimpleRequest.Runtime.Cookies;

public interface IRequestCookies {
    IEnumerable<string> Keys { get; }
    
    bool ContainsKey(string key);
    
    bool TryGetValue(string key, out string? value);

    string? GetValueOrDefault(string key, string? defaultValue = null) {
        return TryGetValue(key, out var value) ? value : defaultValue;
    }
}

public class RequestCookies : IRequestCookies {
    private readonly Dictionary<string, string> _cookies = new();
    
    public RequestCookies() {
        
    }

    public RequestCookies(IEnumerable<KeyValuePair<string, string>> cookies) {
        _cookies = cookies.ToDictionary(x => x.Key, x => x.Value);
    }
    
    public IEnumerable<string> Keys => _cookies.Keys;

    public bool ContainsKey(string key) => _cookies.ContainsKey(key);

    public bool TryGetValue(string key, out string? value) {
        return _cookies.TryGetValue(key, out value);
    }
}
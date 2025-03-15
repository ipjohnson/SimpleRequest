using System.Text;

namespace SimpleRequest.Runtime.Cookies;

public interface IResponseCookies {
    void Append(string key, string value);
    
    void Append(string key, string value, CookieOptions options);
    
    void Delete(string key);
}

public record ResponseCookie(string Key, string Value, CookieOptions Options) {
    public void WriteTo(StringBuilder stringBuilder) {
        stringBuilder.Append($"{Key}={Value}");

        Options.WriteTo(stringBuilder);
    }
}

public class ResponseCookies : IResponseCookies {
    private readonly List<ResponseCookie> _cookies = new();
    private static readonly CookieOptions DefaultOptions = 
        new (
            null, 
            null, 
            null, 
            true,
            SameSiteMode.Unspecified, 
            true, 
            TimeSpan.FromDays(90));
    
    public void Append(string key, string value) {
        _cookies.Add(new ResponseCookie(key, value, DefaultOptions));
    }

    public void Append(string key, string value, CookieOptions options) {
        _cookies.Add(new ResponseCookie(key, value, options));
    }

    public void Delete(string key) {
        _cookies.RemoveAll(c => c.Key == key);
    }

    public IEnumerable<ResponseCookie> GetCookies() => _cookies;
}
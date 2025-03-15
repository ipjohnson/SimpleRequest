using System.Text;

namespace SimpleRequest.Runtime.Cookies;

public enum SameSiteMode
{
    Unspecified = -1,
    None = 0,    Lax = 1,
    Strict = 2,
}

public record CookieOptions(
    string? Domain,
    string? Path,
    DateTimeOffset? Expires = null,
    bool Secure = true,
    SameSiteMode SameSite = SameSiteMode.Unspecified,
    bool HttpOnly = true,
    TimeSpan? MaxAge = null) {
    public void WriteTo(StringBuilder stringBuilder) {
        if (Expires != null) {
            stringBuilder.Append($"; Expires={Expires.Value:R}");
        }
        else if (MaxAge != null) {
            stringBuilder.Append($"; Max-Age={MaxAge.Value.TotalSeconds}");
        }
        
        if (Domain != null) {
            stringBuilder.Append($"; Domain={Domain}");
        }
        
        if (Path != null) {
            stringBuilder.Append($"; Path={Path}");
        }

        if (SameSite != SameSiteMode.Unspecified) {
            stringBuilder.Append($"; SameSite=");
            switch (SameSite) {
                case SameSiteMode.None:
                    stringBuilder.Append("None");
                    break;
                case SameSiteMode.Lax:
                    stringBuilder.Append("Lax");
                    break;
                case SameSiteMode.Strict:
                    stringBuilder.Append("Strict");
                    break;
                case SameSiteMode.Unspecified:
                    stringBuilder.Append("Unspecified");
                    break;
            }
        }
                
        if (Secure) {
            stringBuilder.Append($"; Secure");
        }
    }
}
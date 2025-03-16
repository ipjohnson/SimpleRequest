using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Utilities;

namespace SimpleRequest.Runtime.Attributes;

public class HeaderPresentAttribute(string name) : Attribute, IExtendedRouteMatch {

    public string? Value { get; set; }
    
    public bool IsMatch(IRequestContext context) {
        var value = context.RequestData.Headers.GetValueOrDefault(name);

        if (Value != null) {
            return Value.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        
        return value != StringValues.Empty;
    }
}
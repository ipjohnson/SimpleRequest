namespace SimpleRequest.Runtime.Invoke;

/// <summary>
/// Extension to match handler using extended route match attributes
/// </summary>
public static class IExtendedRouteMatchExtensions {
    public static IRequestHandler? ReturnMatch(this IRequestHandler? match, IRequestContext? context) {
        if (context != null && match != null) {
            foreach (var attribute in match.RequestHandlerInfo.Attributes) {
                if (attribute is IExtendedRouteMatch routeMatch && !routeMatch.IsMatch(context)) {
                    return null;
                }
            }
        }

        return match;
    }
}

/// <summary>
/// Attributes implement this interface to participate
/// in the final route matching decision.
/// </summary>
public interface IExtendedRouteMatch {
    bool IsMatch(IRequestContext context);
}
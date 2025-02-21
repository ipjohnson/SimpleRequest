using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Logging;

public interface IRequestLoggingDataProvider {
    IReadOnlyList<RequestLoggingData> GetRequestLoggingData(IRequestContext context);
} 

[SingletonService]
public class RequestLoggingDataProvider : IRequestLoggingDataProvider {
    
    public IReadOnlyList<RequestLoggingData> GetRequestLoggingData(IRequestContext context) {
        return Array.Empty<RequestLoggingData>();
    }
}
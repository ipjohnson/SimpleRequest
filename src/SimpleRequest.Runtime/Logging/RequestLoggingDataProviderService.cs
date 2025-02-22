using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Logging;

public interface IRequestLoggingDataProviderService {
    List<RequestLoggingData> GetRequestLoggingData(IRequestContext context);
}

[SingletonService]
public class RequestLoggingDataProviderService(IEnumerable<IRequestLoggingDataProvider> dataProviders) : 
    IRequestLoggingDataProviderService{
    private readonly IReadOnlyList<IRequestLoggingDataProvider> _dataProviders = dataProviders.ToArray();


    public List<RequestLoggingData> GetRequestLoggingData(IRequestContext context) {
        var data = new List<RequestLoggingData>();

        for (int i = 0; i < _dataProviders.Count; i++) {
            var provider = _dataProviders[i];
            data.AddRange(provider.GetRequestLoggingData(context));
        }
        return data;
    }
}
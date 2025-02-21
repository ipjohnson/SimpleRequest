using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Filters;

public class IoRequestFilter : IRequestFilter {
    private readonly IRequestContextSerializer _requestContextSerializer;
    private readonly IRequestLoggingDataProvider _requestLoggingDataProvider;

    public IoRequestFilter(
        IRequestContextSerializer requestContextSerializer,
        IRequestLoggingDataProvider requestLoggingDataProvider) {
        _requestContextSerializer = requestContextSerializer;
        _requestLoggingDataProvider = requestLoggingDataProvider;
    }

    public async Task Invoke(IRequestChain requestChain) {
        var context = requestChain.Context;
        var deserializeStartTime = MachineTimestamp.Now;

        try {
            await _requestContextSerializer.DeserializeToParameters(requestChain.Context);
        }
        catch (Exception e) {
            context.ResponseData.ExceptionValue = e;
            context.RequestLogger.RequestParameterBindFailed(context, e);
        }
        finally {
            context.MetricLogger.Record(
                RequestMetrics.ParameterBindDuration, deserializeStartTime);
        }

        var loggingScope = CreateLoggingScope(context);
        
        if (context.ResponseData.ExceptionValue == null) {
            await requestChain.Next();
        }
        
        var responseStartTime = MachineTimestamp.Now;

        try {
            await _requestContextSerializer.SerializeToResponse(requestChain.Context);
        }
        catch (Exception e) {
            context.ResponseData.ExceptionValue = e;
        }
        finally {
            context.MetricLogger.Record(RequestMetrics.ResponseDuration, responseStartTime);
            loggingScope?.Dispose();
        }
    }

    private IDisposable? CreateLoggingScope(IRequestContext context) {
        var loggingData = 
            _requestLoggingDataProvider.GetRequestLoggingData(context);

        if (loggingData.Count > 0) {
            foreach (var requestLoggingData in loggingData) {
                if ((requestLoggingData.Feature & LoggingDataFeature.MetricData) == 
                    LoggingDataFeature.MetricData) {
                    context.MetricLogger.Data(requestLoggingData.Key, requestLoggingData.Value);
                }
                
                if ((requestLoggingData.Feature & LoggingDataFeature.MetricTag) == 
                    LoggingDataFeature.MetricTag) {
                    context.MetricLogger.Tag(requestLoggingData.Key, requestLoggingData.Value);
                }
            }
            
            return context.RequestLogger.Instance.BeginScope(
                loggingData.Where(
                    d => (d.Feature & LoggingDataFeature.LogData) == LoggingDataFeature.LogData).Select(d => d.AsKeyValuePair()));
        }
        
        return null;
    }
}
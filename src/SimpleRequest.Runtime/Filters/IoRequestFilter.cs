using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Filters;

public class IoRequestFilter(IRequestContextSerializer requestContextSerializer,
    IRequestLoggingDataProviderService requestLoggingDataProviderService,
    ILoggingContextAccessor? loggingContextAccessor)
    : IRequestFilter {

    public async Task Invoke(IRequestChain requestChain) {
        var context = requestChain.Context;
        var deserializeStartTime = MachineTimestamp.Now;

        try {
            await requestContextSerializer.DeserializeToParameters(requestChain.Context);
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
            await requestContextSerializer.SerializeToResponse(requestChain.Context);
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
            requestLoggingDataProviderService.GetRequestLoggingData(context);

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

            if (loggingContextAccessor != null) {
                return context.RequestLogger.Instance.BeginScope(
                    loggingData.Where(
                        d => (d.Feature & LoggingDataFeature.LogData) == LoggingDataFeature.LogData).Select(d => d.AsKeyValuePair()));
            }
            
            loggingContextAccessor?.SetList(loggingData);
        }

        return null;
    }
}
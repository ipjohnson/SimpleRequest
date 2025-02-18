using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Filters;

public class IoRequestFilter : IRequestFilter {
    private readonly IRequestContextSerializer _requestContextSerializer;

    public IoRequestFilter(IRequestContextSerializer requestContextSerializer) {
        _requestContextSerializer = requestContextSerializer;
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
        }
    }
}
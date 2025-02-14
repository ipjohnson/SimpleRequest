using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Runtime.Filters;

public class InvokeFilter : IRequestFilter {

    public async Task Invoke(IRequestChain requestChain) {
        var context = requestChain.Context;
        var startTime = MachineTimestamp.Now;

        try {
            if (context.RequestHandlerInfo != null) {
                await context.RequestHandlerInfo.InvokeInfo.InvokeHandler(context);
            }
        }
        catch (Exception e) {
            requestChain.Context.ResponseData.ExceptionValue = e;
        }
        finally {
            context.MetricLogger.Record(RequestMetrics.HandlerInvokeDuration, startTime);
        }
    }
}
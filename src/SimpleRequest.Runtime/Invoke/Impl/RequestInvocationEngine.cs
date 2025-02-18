using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Runtime.Invoke.Impl;

[SingletonService]
public class RequestInvocationEngine : IRequestInvocationEngine {
    private readonly IReadOnlyList<IRequestHandlerProvider> _providers;
    private readonly INotFoundHandler _notFoundHandler;

    public RequestInvocationEngine(
        IEnumerable<IRequestHandlerProvider> providers, 
        INotFoundHandler notFoundHandler) {
        _notFoundHandler = notFoundHandler;
        _providers = providers.Reverse().ToArray();
    }

    public async Task Invoke(IRequestContext context) {
        var logger = context.RequestLogger;
        var startTime = MachineTimestamp.Now;
        
        logger.RequestBegin(context);
        
        for (var i = 0; i < _providers.Count; i++) {
            var handler = _providers[i].GetRequestHandler(context);

            if (handler != null) {
                context.RequestHandlerInfo = handler.RequestHandlerInfo;
                
                logger.RequestMapped(context);
                
                try {
                    await handler.Invoke(context);
                }
                catch (Exception ex) {
                    context.ResponseData.ExceptionValue = ex;
                }
                finally {
                    if (context.ResponseData.ExceptionValue != null) {
                        logger.RequestFailed(context, context.ResponseData.ExceptionValue);
                    }
                    
                    logger.RequestEnd(context);
                }
                
                return;
            }
        }
        
        await _notFoundHandler.Handle(context);
        logger.ResourceNotFound(context);
        context.MetricLogger.Record(RequestMetrics.TotalRequestDuration, startTime);
    }
}
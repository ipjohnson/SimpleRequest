using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Runtime.Invoke.Impl;

[SingletonService]
public class RequestInvocationEngine(
    IRequestHandlerLocator handlerLocator,
    INotFoundHandler notFoundHandler) : IRequestInvocationEngine {

    public async Task Invoke(IRequestContext context) {
        var logger = context.RequestLogger;
        var startTime = MachineTimestamp.Now;
        
        logger.RequestBegin(context);

        var handler = handlerLocator.GetHandler(context);

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

        await notFoundHandler.Handle(context);
        
        logger.ResourceNotFound(context);
        context.MetricLogger.Record(RequestMetrics.TotalRequestDuration, startTime);
    }
}
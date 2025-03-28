using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IRequestErrorHandler {
    Task HandleError(IRequestContext context);
}

[SingletonService]
public class RequestErrorHandler(IExceptionProcessor exceptionProcessor) : IRequestErrorHandler {

    public async Task HandleError(IRequestContext context) {
        if (context.ResponseData.ExceptionValue != null) {
            await exceptionProcessor.Process(context, context.ResponseData.ExceptionValue);
        }
        
        await (context.ResponseData.TemplateName != null ? 
            HandleErrorWithTemplate(context) : 
            HandleErrorData(context));
    }

    protected virtual Task HandleErrorData(IRequestContext context) {
        if (context.ResponseData.ResponseValue == null) {
            context.ResponseData.ResponseValue = new {
                Error = "An unexpected error has occurred."
            };
        }

        return Task.CompletedTask;
    }
    
    protected virtual Task HandleErrorWithTemplate(IRequestContext context) {
        context.ResponseData.TemplateName = "error-template";
        if (context.ResponseData.ResponseValue == null) {
            context.ResponseData.ResponseValue = new {
                Error = "An unexpected error has occurred."
            };
        }

        return Task.CompletedTask;
    }
}
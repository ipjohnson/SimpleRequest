using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IRequestErrorHandler {
    Task HandleError(IRequestContext context);
}

[SingletonService]
public class RequestErrorHandler : IRequestErrorHandler {

    public Task HandleError(IRequestContext context) {
        if (context.ResponseData.ExceptionValue != null) {
            context.ResponseData.Status = GetStatusCodeForException(context, context.ResponseData.ExceptionValue);
        }
        
        return context.ResponseData.TemplateName != null ? 
            HandleErrorWithTemplate(context) : 
            HandleErrorData( context);
    }

    protected virtual Task HandleErrorData(IRequestContext context) {
        context.ResponseData.ResponseValue = new {
            Error = "An unexpected error has occurred."
        };
        
        return Task.CompletedTask;
    }
    
    protected virtual Task HandleErrorWithTemplate(IRequestContext context) {
        context.ResponseData.TemplateName = "error-template";
        context.ResponseData.ResponseValue = new {
            Error = "An unexpected error has occurred."
        };
        
        return Task.CompletedTask;
    }

    protected virtual int GetStatusCodeForException(IRequestContext context, Exception exception) {
        switch (exception) {
            default:
                return 500;
        }
    }
}
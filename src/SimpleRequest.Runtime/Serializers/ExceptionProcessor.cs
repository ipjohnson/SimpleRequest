using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Exceptions;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IExceptionProcessor {
    Task Process(IRequestContext context, Exception exception);
}

[SingletonService]
public class ExceptionProcessor : IExceptionProcessor {

    public Task Process(IRequestContext context, Exception exception) {
        var data = MapStatusCode(exception);

        if (data != null) {
            context.ResponseData.Status = data.Value.Item1;
            context.ResponseData.ResponseValue = data.Value.Item2;
            
            return Task.CompletedTask;
        }

        if (exception is IRequestException requestException) {
            context.ResponseData.Status = requestException.StatusCode;
            context.ResponseData.ResponseValue = requestException.ResponseData(context);
        }

        context.ResponseData.Status = 500;
        
        return Task.CompletedTask;
    }
    
    protected virtual (int, object?)? MapStatusCode(Exception exception) {
        return null;
    }
}
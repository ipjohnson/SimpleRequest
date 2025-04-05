using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Models;
using SimpleRequest.Runtime.Pools;

namespace SimpleRequest.Runtime.Serializers;

public interface IRequestContextSerializer {
    Task DeserializeToParameters(IRequestContext context);

    Task SerializeToResponse(IRequestContext context);
}

[SingletonService]
public class RequestContextSerializer : IRequestContextSerializer {
    private readonly RequestResponseConfiguration _responseConfiguration;
    private readonly IContentSerializerManager _contentSerializerManager;
    private readonly IStringBuilderPool _stringBuilderPool;
    private readonly IRequestErrorHandler _requestErrorHandler;

    public RequestContextSerializer(
        RequestResponseConfiguration responseConfiguration,
        IContentSerializerManager contentSerializerManager,
        IStringBuilderPool stringBuilderPool,
        IRequestErrorHandler requestErrorHandler) {
        _responseConfiguration = responseConfiguration;
        _contentSerializerManager = contentSerializerManager;
        _stringBuilderPool = stringBuilderPool;
        _requestErrorHandler = requestErrorHandler;
    }

    public async Task DeserializeToParameters(IRequestContext context) {
        if (context.RequestHandlerInfo != null) {
            await context.RequestHandlerInfo.InvokeInfo.BindParameters(context);
        }
    }

    public async Task SerializeToResponse(IRequestContext context) {
        if (context.ResponseData.ResponseStarted) {
            return;
        }

        if (context.ResponseData.ExceptionValue != null) {
            await _requestErrorHandler.HandleError(context);
        }

        if (context.ResponseData.ResponseStarted) {
            return;
        }

        context.ResponseData.ResponseStarted = true;
        
        if (!string.IsNullOrEmpty(context.ResponseData.TemplateName)) {
            await WriteTemplateOutput(context);
        }
        else if (context.ResponseData.ResponseValue is IContentResult contentResult) {
            await WriteContentResult(context, contentResult);
        }
        else if (context.ResponseData.ResponseValue != null) {
            await OutputSerializedData(context);
        }
    }

    private async Task WriteContentResult(IRequestContext context, IContentResult contentResult) {
        context.ResponseData.Status = contentResult.StatusCode ??
                                      context.RequestHandlerInfo?.SuccessStatus ??
                                      200;
        
        context.ResponseData.ContentType = contentResult.ContentType;
        context.ResponseData.IsBinary = contentResult.IsBinary;

        if (context.ResponseData.Body != null) {
            await context.ResponseData.Body.WriteAsync(contentResult.Content);
        }
    }

    private async Task OutputSerializedData(IRequestContext context) {
        var contentType = context.ResponseData.ContentType ??
                          GetContentTypeFromResponse(context);
        
        var serializer = _contentSerializerManager.GetSerializer(contentType);

        if (serializer != null && context.ResponseData.Body != null) {
            context.ResponseData.ContentType = serializer.ContentType;

            await serializer.SerializeAsync(
                context.ResponseData.Body,
                context.ResponseData.ResponseValue!,
                context.ResponseData.Headers,
                context.CancellationToken);
        }
        else {
            // log error
        }
    }

    private string? GetContentTypeFromResponse(IRequestContext context) {
        if (!string.IsNullOrEmpty(context.RequestData.ContentType)) {
            return context.RequestData.ContentType;
        }
        
        if (context.ResponseData.ResponseValue is string ||
            context.ResponseData.ResponseValue is IAsyncEnumerable<string>) {
            return _responseConfiguration.DefaultStingContentType;
        }

        return null;
    }

    private Task WriteTemplateOutput(IRequestContext context) {
        return context.RequestServices.TemplateInvocationEngine.Invoke(context);
    }
}
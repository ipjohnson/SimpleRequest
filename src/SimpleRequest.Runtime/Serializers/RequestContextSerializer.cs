using CompiledTemplateEngine.Runtime.Engine;
using CompiledTemplateEngine.Runtime.Interfaces;
using CompiledTemplateEngine.Runtime.Utilities;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IRequestContextSerializer {
    Task DeserializeToParameters(IRequestContext context);

    Task SerializeToResponse(IRequestContext context);
}

[SingletonService]
public class RequestContextSerializer : IRequestContextSerializer {
    private readonly IContentSerializerManager _contentSerializerManager;
    private readonly ITemplateExecutionService _templateExecutionService;
    private readonly IStringBuilderPool _stringBuilderPool;
    private readonly IRequestErrorHandler _requestErrorHandler;

    public RequestContextSerializer(
        IContentSerializerManager contentSerializerManager,
        ITemplateExecutionService templateExecutionService,
        IStringBuilderPool stringBuilderPool,
        IRequestErrorHandler requestErrorHandler) {
        _contentSerializerManager = contentSerializerManager;
        _templateExecutionService = templateExecutionService;
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

        if (!string.IsNullOrEmpty(context.ResponseData.TemplateName)) {
            await WriteTemplateOutput(context);
        }
        else if (context.ResponseData.ResponseValue != null) {
            await OutputSerializedData(context);
        }
    }

    private async Task OutputSerializedData(IRequestContext context) {
        var serializer = _contentSerializerManager.GetSerializer(
            context.ResponseData.ContentType ??
            context.RequestData.ContentType);

        if (serializer != null && context.ResponseData.Body != null) {
            context.ResponseData.ContentType = serializer.ContentType;

            await serializer.Serialize(
                context.ResponseData.Body,
                context.ResponseData.ResponseValue!);
        }
        else {
            // log error
        }
    }

    private async Task WriteTemplateOutput(IRequestContext context) {
        using var poolItem = _stringBuilderPool.Get();

        var output = new StringBuilderTemplateOutputWriter(poolItem.Item, context.ResponseData.Body);

        await _templateExecutionService.Execute(context.ResponseData.TemplateName!, context.ResponseData.ResponseValue);

        await output.FlushWriter();
    }
}
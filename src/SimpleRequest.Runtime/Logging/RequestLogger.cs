using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Logging;

[SingletonService]
public partial class RequestLogger : IRequestLogger {
    private readonly ILogger<RequestLogger> _logger;

    public RequestLogger(ILogger<RequestLogger> logger) {
        _logger = logger;
    }

    public void RequestBegin(IRequestContext context) {
        LogRequestStarted(context.RequestData.Method, context.RequestData.Path);
    }

    public void RequestMapped(IRequestContext context) {
        LogRequestMapped(
            context.RequestData.Method,
            context.RequestData.Path,
            context.RequestHandlerInfo?.HandlerType.Name ?? string.Empty,
            context.RequestHandlerInfo?.InvokeInfo.InvokeMethod ?? string.Empty);
    }

    public void RequestEnd(IRequestContext context) {
        LogRequestFinished(
            context.RequestData.Method,
            context.RequestData.Path,
            context.ResponseData.Status,
            context.RequestData.StartTime.GetElapsedTime()
        );
    }

    public void RequestParameterBindFailed(IRequestContext context, Exception? exp) {
        _logger.LogError(exp, "{method} {path} failed to bind parameters",
            context.RequestData.Method, context.RequestData.Path);
    }

    public void RequestFailed(IRequestContext context, Exception exp) {
        _logger.LogError(exp, "{method} {path} request failed", context.RequestData.Method, context.RequestData.Path);
    }

    public void ResourceNotFound(IRequestContext context) {
        LogResourceNotFound(context.RequestData.Method, context.RequestData.Path);
    }

    [LoggerMessage(
        EventId = 78000,
        Level = LogLevel.Information,
        Message = "{httpMethod} {path} started")]
    protected partial void LogRequestStarted(string httpMethod, string path);

    [LoggerMessage(
        EventId = 78001,
        Level = LogLevel.Information,
        Message = "{httpMethod} {path} mapped to {typeName}.{methodName}")]
    protected partial void LogRequestMapped(string httpMethod, string path, string typeName, string methodName);

    [LoggerMessage(
        EventId = 78002,
        Level = LogLevel.Information,
        Message = "{httpMethod} {path}  finished status code '{statusCode}'  duration {durationMs}")]
    protected partial void LogRequestFinished(
        string httpMethod, string path, int? statusCode, TimeSpan durationMs);

    [LoggerMessage(
        EventId = 78003,
        Level = LogLevel.Information,
        Message = "{httpMethod} {path} Resource Not Found")]
    protected partial void LogResourceNotFound(string httpMethod, string path);
}
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Filters;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;

namespace SimpleRequest.Web.Runtime.StaticContentImpl;

public class StaticContentProvider(
    IServiceProvider serviceProvider,
    IRequestFilterManagementService filterManagementService,
    IStaticContentResponseService staticContentResponseService,
    StaticContentProviderConfiguration contentModel,
    IInvokeFilterProvider invokeFilterProvider,
    ILogger<StaticContentProvider> logger
) : IRequestHandlerProvider {
    private ConcurrentDictionary<string, IRequestHandler> _cache = new();

    public int Order => 100;

    /// <summary>
    /// Don't report any handlers 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IRequestHandler> GetHandlers() {
        yield break;
    }

    public IRequestHandler? GetRequestHandler(IRequestContext context) {
        // do not attempt to load paths with ..
        if (context.RequestData.Path.Contains("..")) {
            return null;
        }

        if (_cache.TryGetValue(context.RequestData.Path, out var cacheEntry)) {
            return cacheEntry;
        }

        var handler = FindRequestHandler(context);

        if (handler != null) {
            _cache.TryAdd(context.RequestData.Path, handler);
        }

        return handler;
    }

    private IRequestHandler? FindRequestHandler(IRequestContext context) {
        var filePath = context.RequestData.Path.Replace(contentModel.RequestPath, "");

        if (filePath == "") {
            filePath = contentModel.DefaultFileName;
        }

        try {
            var loadPath = GetType().Assembly.Location;

            var finalPath = Path.Combine(
                Path.GetDirectoryName(loadPath)!,
                contentModel.ContentRootPath,
                filePath);

            if (File.Exists(finalPath)) {
                return CreateRequestHandlerInfo(context, finalPath);
            } 
            
            if (!string.IsNullOrEmpty(contentModel.DefaultFileName) && 
                context.RequestData.Path.StartsWith(contentModel.RequestPath)) {
                finalPath = Path.Combine(
                    Path.GetDirectoryName(loadPath)!,
                    contentModel.ContentRootPath,
                    contentModel.DefaultFileName);
                return CreateRequestHandlerInfo(context, finalPath);
            }
        }
        catch (Exception e) {
            logger.LogError(e, "Error loading static content");
        }
        
        return null;
    }

    private IRequestHandler? CreateRequestHandlerInfo(IRequestContext context, string finalPath) {
        var handlerInfo =
            new StaticHandlerInfo(
                context.RequestData.Path, 
                finalPath,
                staticContentResponseService,
                contentModel);

        return new RequestHandler(
            handlerInfo,
            filterManagementService.GetFiltersWithoutDefaults(handlerInfo,  
                invokeFilterProvider.ProvideFilter(serviceProvider, handlerInfo))
        );
    }
}
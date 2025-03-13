using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Caching.Impl;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Pools;
using SimpleRequest.Runtime.Utilities;

namespace SimpleRequest.Caching;

public enum ResponseCacheLocation {
    Any = 0,
    Client = 1,
    None
}

[RequestFilter(Order = -5000)]
[SingletonService(ServiceType = typeof(ResponseCache))]
public class ResponseCache(
    ICacheProfileProvider cacheProfileProvider,
    IMemoryCache memoryCache,
    IMemoryStreamPool memoryStreamPool,
    IStringBuilderPool stringBuilderPool) : IRequestFilter, ICacheConfiguration {
    private ICacheConfiguration? _configuration = null;

    public string? Profile { get; set; }

    public double Timeout { get; set; } = 60;

    public string[]? VaryByHeader { get; set; }

    public string[]? VaryByQuery { get; set; }

    public string[]? PreserveHeader { get; set; }

    public ResponseCacheLocation Location { get; set; } = ResponseCacheLocation.Any;

    public bool NoStore { get; set; } = false;

    public Task Invoke(IRequestChain requestChain) {
        var keyString = GetKeyString(requestChain.Context);

        if (memoryCache.TryGetValue(keyString, out var result) && result is CacheEntry entry) {
            return WriteCacheEntry(requestChain, entry);
        }

        return InvokeAndCache(requestChain, keyString);
    }

    private async Task InvokeAndCache(IRequestChain requestChain, string keyString) {
        using var memoryStreamItem = memoryStreamPool.Get();
        var response = requestChain.Context.ResponseData;

        var currentBody = response.Body;
        response.Body = memoryStreamItem.Item;

        await requestChain.Next();

        var bytes = memoryStreamItem.Item.ToArray();

        if (response.Status is >= 200 and < 300) {
            var entry =
                new CacheEntry(
                    bytes,
                    response.Status.Value,
                    response.ContentType ?? "",
                    response.Headers.GetValueOrDefault("Content-Encoding"));

            memoryCache.Set(keyString,
                entry,
                TimeSpan.FromMinutes(Timeout));
        }

        if (currentBody != null) {
            await currentBody.WriteAsync(bytes);
        }
    }

    private async Task WriteCacheEntry(IRequestChain requestChain, CacheEntry entry) {
        var response = requestChain.Context.ResponseData;

        response.ContentType = entry.ContentType;

        if (entry.ContentEncoding.Count > 0) {
            response.Headers["Content-Encoding"] = entry.ContentEncoding;
        }

        if (response.Body != null) {
            await response.Body.WriteAsync(entry.Data);
        }
    }

    private string GetKeyString(IRequestContext requestChainContext) {
        var requestData = requestChainContext.RequestData;
        var config = CacheConfiguration;

        if (config.VaryByHeader == null && config.VaryByQuery == null) {
            return string.Concat("cache-", requestChainContext.RequestData.Path, "|", requestChainContext.RequestData.Method);
        }

        using var stringBuilderReservation = stringBuilderPool.Get();
        var builder = stringBuilderReservation.Item;
        builder.Append("cache-");
        builder.Append(requestChainContext.RequestData.Path);
        builder.Append('|');
        builder.Append(requestChainContext.RequestData.Method);

        if (config.VaryByHeader != null) {
            foreach (var varyHeader in config.VaryByHeader) {
                builder.Append('|');
                builder.Append(varyHeader);
                builder.Append('-');
                builder.Append(varyHeader);
            }
        }

        if (config.VaryByQuery != null) {
            foreach (var queryString in config.VaryByQuery) { }
        }
        
        return builder.ToString();
    }

    private ICacheConfiguration CacheConfiguration {
        get {
            if (_configuration != null) return _configuration;

            if (string.IsNullOrEmpty(Profile)) {
                return _configuration = this;
            }

            return _configuration = cacheProfileProvider.GetProfile(Profile);
        }
    }

    private record CacheEntry(
        byte[] Data,
        int StatusCode,
        string ContentType,
        StringValues ContentEncoding);
}
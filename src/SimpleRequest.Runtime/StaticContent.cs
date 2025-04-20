using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Filters;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.StaticContentImpl;

namespace SimpleRequest.Runtime;

[DependencyModule(OnlyRealm = true)]
public partial class StaticContent : IServiceCollectionConfiguration {
    public string? ContentRootPath { get; set; } = "static-content";

    public string? RequestPath { get; set; } = "/";

    public string? DefaultFileName { get; set; } = "";

    public int? MaxAge { get; set; } = 604800; // 7 days

    public bool? SupportCompression { get; set; } = true;

    public bool? CacheCompressedFiles { get; set; } = true;
    
    public bool? NoCacheIndexHtml { get; set; } = true;
    

    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<IRequestHandlerProvider>(
            provider => new StaticContentProvider(
                provider,
                provider.GetRequiredService<IRequestFilterManagementService>(),
                provider.GetRequiredService<IStaticContentResponseService>(),
                new StaticContentProviderConfiguration(
                    ContentRootPath ?? "static-content",
                    RequestPath ?? "/",
                    DefaultFileName ?? "",
                    MaxAge.GetValueOrDefault(604800),
                    SupportCompression.GetValueOrDefault(true), 
                    CacheCompressedFiles.GetValueOrDefault(true),
                    NoCacheIndexHtml.GetValueOrDefault(true)
                ),
                provider.GetRequiredService<IInvokeFilterProvider>(),
                provider.GetRequiredService<ILogger<StaticContentProvider>>()
            ));
    }

    public override bool Equals(object? obj) {
        return obj is StaticContent other &&
               ContentRootPath == other.ContentRootPath &&
               RequestPath == other.RequestPath;
    }

    public override int GetHashCode() {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return (ContentRootPath?.GetHashCode() ?? 13) ^ (RequestPath?.GetHashCode() ?? 39);
    }
}
namespace SimpleRequest.Web.Runtime.StaticContentImpl;

public record StaticContentProviderConfiguration(string ContentRootPath,
    string RequestPath,
    string DefaultFileName,
    int MaxAge,
    bool SupportCompression,
    bool CacheCompressedFiles,
    bool NoCacheIndexHtml);
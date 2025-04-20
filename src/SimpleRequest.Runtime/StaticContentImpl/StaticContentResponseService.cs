using System.IO.Compression;
using System.IO.Hashing;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Pools;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.StaticContentImpl;

public class StaticContentStorage {
    public StaticContentStorage(string filePath) {
        FilePath = filePath;
    }

    public string FilePath { get; set; }
    
    public string? ETag { get; set; }
    
    public string? ContentType { get; set; }
    
    public string? ContentEncoding { get; set; }
    
    public byte[]? Content { get; set; }
}

public interface IStaticContentResponseService {
    Task Invoke(
        IRequestContext context,
        StaticContentProviderConfiguration staticContentModel,
        StaticContentStorage storage);
}

[SingletonService]
public class StaticContentResponseService(
    IMemoryStreamPool memoryStreamPool,
    IStreamCompressionService compressionService,
    IContentTypeHelper contentTypeHelper) : IStaticContentResponseService {

    public Task Invoke(
        IRequestContext context,
        StaticContentProviderConfiguration staticContentModel,
        StaticContentStorage storage) {
        if (storage.Content != null) {
            return ReturnCachedCopy(context, staticContentModel, storage);
        }
        
        return ReturnContentFromDisk(context, staticContentModel, storage);
    }

    private Task ReturnContentFromDisk(
        IRequestContext context, 
        StaticContentProviderConfiguration staticContentModel, 
        StaticContentStorage storage) {

        if (staticContentModel.SupportCompression) {
            return GetCompressedContent(context, staticContentModel, storage);
        }
        else {
            return GetUncompressedContent(context, staticContentModel, storage);
        }
    }

    private async Task GetUncompressedContent(IRequestContext context, StaticContentProviderConfiguration staticContentModel, StaticContentStorage storage) {
        var contentType = 
            contentTypeHelper.GetContentTypeFromExtension(Path.GetExtension(storage.FilePath));
        using var memoryStreamReservation = memoryStreamPool.Get();
        await using var fileStream = File.OpenRead(storage.FilePath);
        await fileStream.CopyToAsync(memoryStreamReservation.Item);
        
        storage.Content = memoryStreamReservation.Item.ToArray();
        storage.ContentType = contentType;
        storage.ETag = CalculateETag(storage.Content);
        
        await ReturnCachedCopy(context, staticContentModel, storage);
    }

    private async Task GetCompressedContent(IRequestContext context, StaticContentProviderConfiguration staticContentModel, StaticContentStorage storage) {
        var contentType = 
            contentTypeHelper.GetContentTypeFromExtension(Path.GetExtension(storage.FilePath));
        using var memoryStreamReservation = memoryStreamPool.Get();
        await using var gzipStream = compressionService.GetStream(
            memoryStreamReservation.Item, "gzip", CompressionMode.Compress);
        
        await using var fileStream = File.OpenRead(storage.FilePath);

        if (gzipStream != null) {
            storage.ContentEncoding = "gzip";
            await fileStream.CopyToAsync(gzipStream!);
        }
        else {
            await fileStream.CopyToAsync(memoryStreamReservation.Item);
        }

        storage.Content = memoryStreamReservation.Item.ToArray();
        storage.ContentType = contentType;
        storage.ETag = CalculateETag(storage.Content);
        
        await ReturnCachedCopy(context, staticContentModel, storage);
    }

    private string? CalculateETag(byte[] storageContent) {
        var crc32 = new Crc32();
        
        crc32.Append(storageContent);

        return Convert.ToBase64String(crc32.GetHashAndReset());
    }

    private async Task ReturnCachedCopy(
        IRequestContext context,
        StaticContentProviderConfiguration staticContentModel,
        StaticContentStorage storage) {
        if (storage.ETag != null &&
            context.RequestData.Headers.TryGetValue("If-None-Match", out var etag) && etag.Contains(storage.ETag)) {
            context.ResponseData.Status = 304;
            context.ResponseData.Headers.Add("ETag", storage.ETag);

            return;
        }

        if (context.ResponseData.Body == null) {
            return;
        }
        
        context.ResponseData.Status = 200;
        context.ResponseData.ContentType = storage.ContentType;
        
        if (storage.ETag != null) {
            context.ResponseData.Headers.Add("ETag", storage.ETag);
        }

        if (storage.ContentEncoding != null) {
            if (context.RequestData.Headers.TryGetValue("Accept-Encoding", out var encoding) &&
                encoding.Contains(storage.ContentEncoding)) {
                context.ResponseData.Headers.Add("Content-Encoding", storage.ContentEncoding);

                await context.ResponseData.Body.WriteAsync(storage.Content);
            }
            else {
                await using var compression = 
                    compressionService.GetStream(context.ResponseData.Body, storage.ContentEncoding, CompressionMode.Decompress);

                if (compression != null) {
                    await compression.CopyToAsync(context.ResponseData.Body, context.CancellationToken);
                }
            }
        }
        else {
            await context.ResponseData.Body.WriteAsync(storage.Content);
        }
    }
}
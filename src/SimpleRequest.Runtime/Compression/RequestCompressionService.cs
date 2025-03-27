using System.IO.Compression;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Compression;

public interface IRequestCompressionService {
    IAsyncDisposable? CompressResponse(IRequestContext requestContext);

    IAsyncDisposable? DecompressRequest(IRequestContext requestContext);
}

[SingletonService]
public class RequestCompressionService(IStreamCompressionService compressionService) : IRequestCompressionService {
    private const string ContentEncoding = "Content-Encoding";
    private const string AcceptEncoding = "Accept-Encoding";

    public IAsyncDisposable? DecompressRequest(IRequestContext requestContext) {
        if (requestContext.RequestData.Body != null &&
            requestContext.RequestData.Headers.TryGetValue(ContentEncoding, out var encoding)) {

            return compressionService.GetStream(requestContext.RequestData.Body, encoding.ToString(), CompressionMode.Decompress);
        }

        return null;
    }

    public IAsyncDisposable? CompressResponse(IRequestContext context) {
        if (context.ResponseData.Body == null ||
            context.ResponseData.Status.GetValueOrDefault(200) >= 300) {
            return null;
        }

        if (!context.ResponseData.ShouldCompress.GetValueOrDefault(true)) {
            return null;
        }

        if (context.RequestData.Headers.TryGetValue("Accept-Encoding", out var encoding)) {
            foreach (var value in encoding) {
                if (string.IsNullOrEmpty(value)) {
                    continue;
                }
                
                var stream = compressionService.GetStream(
                    context.ResponseData.Body, value, CompressionMode.Compress);

                if (stream != null) {
                    context.ResponseData.Headers[ContentEncoding] = value;
                    context.ResponseData.Body = stream;
                    context.ResponseData.IsBinary = true;

                    return stream;
                }
            }
        }

        return null;
    }
}
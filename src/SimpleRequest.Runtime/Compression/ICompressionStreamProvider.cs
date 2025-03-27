using System.IO.Compression;

namespace SimpleRequest.Runtime.Compression;

public interface ICompressionStreamProvider {
    IEnumerable<string> SupportedContentTypes { get; }
    
    Stream GetStream(Stream stream, CompressionMode mode);
}
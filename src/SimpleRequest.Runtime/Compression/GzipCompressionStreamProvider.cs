using System.IO.Compression;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Compression;

[SingletonService]
public class GzipCompressionStreamProvider : ICompressionStreamProvider {
    public IEnumerable<string> SupportedContentTypes => ["gzip", "gz"];
    
    public Stream GetStream(Stream stream, CompressionMode mode) {
        if (mode == CompressionMode.Decompress) {
            return new GZipStream(stream, CompressionMode.Decompress, true);
        }
        
        return new GZipStream(stream, CompressionLevel.Fastest, true);
    }
}
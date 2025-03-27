using System.IO.Compression;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Compression;

[SingletonService]
public class BrotliCompressionStreamProvider : ICompressionStreamProvider {

    public IEnumerable<string> SupportedContentTypes => ["br"];

    public Stream GetStream(Stream stream, CompressionMode mode) {
        if (mode == CompressionMode.Decompress) {
            return new BrotliStream(stream, CompressionMode.Decompress, true);
        }
        
        return new BrotliStream(stream, CompressionLevel.Fastest, true);
    }
}
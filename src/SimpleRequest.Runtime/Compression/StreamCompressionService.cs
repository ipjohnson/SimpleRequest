using System.IO.Compression;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Compression;

public interface IStreamCompressionService {
    Stream? GetStream(Stream stream, string encoding, CompressionMode mode);
}

[SingletonService]
public class StreamCompressionService : IStreamCompressionService {
    private Dictionary<string, ICompressionStreamProvider> _providers = new();

    public StreamCompressionService(IEnumerable<ICompressionStreamProvider> providers) {
        foreach (var provider in providers) {
            foreach (var type in provider.SupportedContentTypes) {
                _providers[type] = provider;
            }
        }
    }
    
    public Stream? GetStream(Stream stream, string encoding, CompressionMode mode) {
        if (!_providers.TryGetValue(encoding, out var provider)) {
            return null;
        }
        
        return provider.GetStream(stream, mode);
    }
}
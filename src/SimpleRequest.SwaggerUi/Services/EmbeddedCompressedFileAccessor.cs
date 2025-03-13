using System.IO.Compression;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Pools;

namespace SimpleRequest.SwaggerUi.Services;

public interface IEmbeddedCompressedFileAccessor {
    Task<byte[]?> ReadFile(string path);
}

[SingletonService]
public class EmbeddedCompressedFileAccessor(IMemoryStreamPool streamPool) : IEmbeddedCompressedFileAccessor {
    private volatile Dictionary<string, byte[]>? _files;

    public async Task<byte[]?> ReadFile(string path) {
        var files = await Files();

        return files.GetValueOrDefault(path);
    }

    private async Task<Dictionary<string, byte[]>> Files() {
        if (_files != null) {
            return _files;
        }

        return _files = await DecompressFiles();
    }

    private async Task<Dictionary<string, byte[]>> DecompressFiles() {
        var resourceStream =
            GetType().Assembly.GetManifestResourceStream("SimpleRequest.SwaggerUi.swagger_ui.web-assets.zip");

        if (resourceStream == null) {
            throw new FileNotFoundException("Could not find embedded compressed resource.");
        }

        var files = new Dictionary<string, byte[]>();
        using var archive = new ZipArchive(resourceStream, ZipArchiveMode.Read);
        using var memoryStreamPoolItem = streamPool.Get();
        var memoryStream = memoryStreamPoolItem.Item;

        foreach (var entry in archive.Entries) {
            await using var openStream = entry.Open();
            memoryStream.Position = 0;
            memoryStream.SetLength(0);
            
            await openStream.CopyToAsync(memoryStream);
            files.Add(entry.FullName, memoryStream.ToArray());
        }

        return files;
    }
}
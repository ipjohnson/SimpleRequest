using System.Collections;
using System.Text.Json;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;

namespace SimpleRequest.Runtime.Serializers.Json;

[SingletonService]
public class NewLineDelimitedJsonSerializer  : IContentSerializer {

    public int Order => 1_000_000_002;

    public SupportedSerializerFeature Features => SupportedSerializerFeature.SerializeAsync;

    public Task SerializeAsync(Stream stream, object value, IDictionary<string, StringValues>? headers = null, CancellationToken cancellationToken = default) {
        if (value is IAsyncEnumerable<object> asyncEnumerable) {
            return SerializeAsyncEnumerable(stream, asyncEnumerable, headers, cancellationToken);
        }
        
        throw new NotSupportedException("Only IAsyncEnumerable<object> is supported for NewLineDelimitedJsonSerializer.");
    }

    private async Task SerializeAsyncEnumerable(
        Stream stream,
        IAsyncEnumerable<object> asyncEnumerable, 
        IDictionary<string, StringValues>? headers,
        CancellationToken cancellationToken) {
        if (headers != null) {
            headers["Cache-Control"] = "no-cache";
            headers["Connection"] = "keep-alive";
            headers["Transfer-Encoding"] = "chunked";
        }

        await foreach (var obj in asyncEnumerable.WithCancellation(cancellationToken)) {
            JsonSerializer.Serialize(stream, obj);
            stream.Write("\n"u8);
            await stream.FlushAsync(cancellationToken);
        }
    }

    public string Serialize(object value) {
        throw new NotImplementedException();
    }

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type, IDictionary<string, StringValues>? headers = null, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask<T?> DeserializeAsync<T>(Stream stream, IDictionary<string, StringValues>? headers = null, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public object? Deserialize(string stringValue, Type type) {
        throw new NotImplementedException();
    }

    public T? Deserialize<T>(string stringValue) {
        throw new NotImplementedException();
    }

    public bool IsDefault => false;

    public string ContentType => "application/x-ndjson";

    public bool CanSerialize(string contentType) {
        return contentType.StartsWith("application/x-ndjson");
    }
}
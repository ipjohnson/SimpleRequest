using System.Collections;
using System.Text;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Serializers.String;

[SingletonService]
public class StringSerializer : IContentSerializer {

    public async Task Serialize(Stream stream, object value, CancellationToken cancellationToken = default) {
        await using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        
        if (value is string stringValue) {
            await writer.WriteAsync(stringValue);
        }
        else if (value is IAsyncEnumerable<string> asyncEnumerable) {
            await foreach (var enumeratedStringValue in asyncEnumerable.WithCancellation(cancellationToken)) {
                await writer.WriteAsync(enumeratedStringValue);
            }
        }
        else if (value is IEnumerable enumerable) {
            foreach (var obj in enumerable) {
                if (obj is string objectString) {
                    await writer.WriteAsync(objectString);
                }
                else {
                    await writer.WriteAsync(obj.ToString());
                }
            }
        }
    }

    public string Serialize(object value) {
        if (value is string stringValue) {
            return stringValue;
        }
        return value?.ToString() ?? string.Empty;
    }

    public ValueTask<object?> Deserialize(Stream stream, Type type, CancellationToken cancellationToken = default) {
        return new ValueTask<object?>();
    }

    public ValueTask<T?> Deserialize<T>(Stream stream, CancellationToken cancellationToken = default) {
        return new ValueTask<T?>();
    }

    public object? Deserialize(string stringValue, Type type) {
       return stringValue;
    }

    public T? Deserialize<T>(string stringValue) {
        return default;
    }

    public bool IsDefault => false;

    public string ContentType => "text/plain";

    public bool CanSerialize(string contentType) {
        return contentType.StartsWith("text/");
    }
}
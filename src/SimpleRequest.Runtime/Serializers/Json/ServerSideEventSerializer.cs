using System.Collections;
using System.Text.Json;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Models;

namespace SimpleRequest.Runtime.Serializers.Json;

[SingletonService]
public class ServerSideEventSerializer(
    ISystemTextJsonSerializerOptionProvider serializerOptionProvider) : IContentSerializer {

    public int Order => 1_000_000_001;
    
    public SupportedSerializerFeature Features => SupportedSerializerFeature.SerializeAsync;

    public Task SerializeAsync(Stream stream, object value, IDictionary<string,StringValues>? headers = null, CancellationToken cancellationToken = default) {
        
        if (value is IAsyncEnumerable<string> stringValue) {
            SetStreamingHeaders(headers);
            return SerializeAsyncString(stream, stringValue, cancellationToken);
        }

        if (value is IAsyncEnumerable<ServerSideEventModel> eventEnumerable) {
            SetStreamingHeaders(headers);
            return SerializeEventEnumerable(stream, eventEnumerable, cancellationToken);
        }
        
        if (value is IAsyncEnumerable<object> objectValues) {
            SetStreamingHeaders(headers);
            return SerializeObjectEnumerable(stream, objectValues, cancellationToken);
        }

        if (value is ServerSideEventModel eventModel) {
            return SerializeServerSideEventModel(stream, eventModel, cancellationToken);
        }

        return SerializeObject(stream, value, cancellationToken);
    }

    private void SetStreamingHeaders(IDictionary<string, StringValues>? headers) {
        if (headers == null) {
            return;
        }
        
        headers["Cache-Control"] = "no-cache";
        headers["Connection"] = "keep-alive";
        headers["Transfer-Encoding"] = "chunked";
    }

    private async Task SerializeObject(Stream stream, object value, CancellationToken cancellationToken) {
        await stream.WriteStringAsync("data: ", cancellationToken);
        await JsonSerializer.SerializeAsync(
            stream, value, serializerOptionProvider.GetOptions(), cancellationToken);
        await stream.WriteStringAsync("\n\n", cancellationToken);
    }

    private async Task SerializeServerSideEventModel(Stream stream, ServerSideEventModel eventModel, CancellationToken cancellationToken) {
        if (!string.IsNullOrEmpty(eventModel.EventName)) {
            await stream.WriteStringAsync(
                "event: " + eventModel.EventName + "\n", cancellationToken);
        }
            
        await stream.WriteStringAsync("data: ", cancellationToken);
        await JsonSerializer.SerializeAsync(
            stream, eventModel.Data, serializerOptionProvider.GetOptions(), cancellationToken);
        await stream.WriteStringAsync("\n", cancellationToken);

        if (!string.IsNullOrEmpty(eventModel.Id)) {
            await stream.WriteStringAsync("id: " + eventModel.Id + "\n", cancellationToken);
        }

        if (eventModel.Retry.HasValue) {
            await stream.WriteStringAsync("retry: " + eventModel.Retry.Value + "\n", cancellationToken);
        }
            
        await stream.WriteStringAsync("\n", cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private async Task SerializeObjectEnumerable(
        Stream stream, IAsyncEnumerable<object> objectValues, CancellationToken cancellationToken) {
        
        await foreach (var eventModel in objectValues.WithCancellation(cancellationToken)) {
            await stream.WriteStringAsync("data: ", cancellationToken);
            await JsonSerializer.SerializeAsync(
                stream, eventModel, serializerOptionProvider.GetOptions(), cancellationToken);
            
            await stream.WriteStringAsync("\n\n", cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }

    private async Task SerializeEventEnumerable(Stream stream, IAsyncEnumerable<ServerSideEventModel> eventEnumerable, CancellationToken cancellationToken) {
        
        await foreach (var eventModel in eventEnumerable.WithCancellation(cancellationToken)) {
            await SerializeServerSideEventModel(stream, eventModel, cancellationToken);
        }
    }

    private async Task SerializeAsyncString(Stream stream, IAsyncEnumerable<string> stringValue, CancellationToken cancellationToken) {
        await using var writer = new StreamWriter(stream);
        
        await foreach (var enumeratedStringValue in stringValue.WithCancellation(cancellationToken)) {
            await writer.WriteAsync("data: ");
            await writer.WriteAsync(enumeratedStringValue);
            await writer.WriteAsync("\n\n");
            await writer.FlushAsync(cancellationToken);
        }
    }

    public string Serialize(object value) {
        throw new NotSupportedException("Seriailizing Server Side Events is not supported.");
    }

    public async ValueTask<object?> DeserializeAsync(Stream stream, Type type, IDictionary<string,StringValues>? headers = null, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask<T?> DeserializeAsync<T>(Stream stream, IDictionary<string,StringValues>? headers = null, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public object? Deserialize(string stringValue, Type type) {
        throw new NotImplementedException();
    }

    public T? Deserialize<T>(string stringValue) {
        throw new NotImplementedException();
    }

    public bool IsDefault => false;

    public string ContentType => "text/event-stream";

    public bool CanSerialize(string contentType) {
        return contentType.StartsWith("text/event-stream");
    }
}
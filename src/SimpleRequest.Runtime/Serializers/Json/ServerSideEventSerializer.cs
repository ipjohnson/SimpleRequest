using System.Collections;
using System.Text.Json;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Models;
using SimpleRequest.Runtime.Pools;

namespace SimpleRequest.Runtime.Serializers.Json;

[SingletonService]
public class ServerSideEventSerializer(
    IMemoryStreamPool memoryStreamPool,
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
        stream.WriteString("data: ");
        await JsonSerializer.SerializeAsync(
            stream, value, serializerOptionProvider.GetOptions(), cancellationToken);
        stream.WriteString("\n\n");
        await stream.FlushAsync(cancellationToken);
    }

    private void WriteServerSideEventModel(Stream stream, ServerSideEventModel eventModel) {
                
        if (!string.IsNullOrEmpty(eventModel.EventName)) {
            stream.WriteString(
                "event: " + eventModel.EventName + "\n");
        }
            
        stream.WriteString("data: ");
        JsonSerializer.Serialize(stream, eventModel.Data, serializerOptionProvider.GetOptions());
        
        stream.WriteString("\n");

        if (!string.IsNullOrEmpty(eventModel.Id)) {
            stream.WriteString("id: " + eventModel.Id + "\n");
        }

        if (eventModel.Retry.HasValue) {
            stream.WriteString("retry: " + eventModel.Retry.Value + "\n");
        }
            
        stream.WriteString("\n");
    }
    
    private async Task SerializeServerSideEventModel(Stream stream, ServerSideEventModel eventModel, CancellationToken cancellationToken) {
        WriteServerSideEventModel(stream, eventModel);
        
        await stream.FlushAsync(cancellationToken);
    }

    private async Task SerializeObjectEnumerable(
        Stream stream, IAsyncEnumerable<object> objectValues, CancellationToken cancellationToken) {
        using var memoryStreamRes = memoryStreamPool.Get();
        
        var memoryStream = memoryStreamRes.Item;
        var utf8Writer = new Utf8JsonWriter(memoryStream);
        
        await foreach (var eventModel in objectValues.WithCancellation(cancellationToken)) {
            memoryStream.SetLength(0);
            utf8Writer.Reset();
            
            memoryStream.WriteString("data: ");
            
            JsonSerializer.Serialize(utf8Writer, eventModel, serializerOptionProvider.GetOptions());
            
            utf8Writer.Flush();
            
            stream.WriteString("\n\n");
            stream.Position = 0;
            
            await memoryStream.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }

    private async Task SerializeEventEnumerable(Stream stream, IAsyncEnumerable<ServerSideEventModel> eventEnumerable, CancellationToken cancellationToken) {
        using var memoryStreamRes = memoryStreamPool.Get();
        var memoryStream = memoryStreamRes.Item;
        
        await foreach (var eventModel in eventEnumerable.WithCancellation(cancellationToken)) {
            memoryStream.SetLength(0);
            
            WriteServerSideEventModel(memoryStream, eventModel);
            
            memoryStream.Position = 0;
            
            await stream.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }

    private async Task SerializeAsyncString(Stream stream, IAsyncEnumerable<string> stringValue, CancellationToken cancellationToken) {
        using var memoryStreamRes = memoryStreamPool.Get();
        var memoryStream = memoryStreamRes.Item;
        
        await foreach (var enumeratedStringValue in stringValue.WithCancellation(cancellationToken)) {
            memoryStream.SetLength(0);
            memoryStream.WriteString("data: ");
            
            memoryStream.WriteString(enumeratedStringValue);
            memoryStream.WriteString("\n\n");
            
            memoryStream.Position = 0;
            
            await stream.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
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
using System.Text.Json;

namespace SimpleRequest.Runtime.Serializers.Json;

public interface ISystemTextJsonConfiguration {
    void ConfigureJson(JsonSerializerOptions options);
}
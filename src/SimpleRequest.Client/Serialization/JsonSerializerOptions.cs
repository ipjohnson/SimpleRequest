using System.Text.Json;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Client.Serialization;

public interface ISystemTextJsonSerializerOptions {
    JsonSerializerOptions Options { get; }
}

[SingletonService(Using = RegistrationType.Try)]
public class SystemTextJsonSerializerOptions : ISystemTextJsonSerializerOptions {

    public SystemTextJsonSerializerOptions() {
        Options = new JsonSerializerOptions {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public JsonSerializerOptions Options {
        get;
    }
}
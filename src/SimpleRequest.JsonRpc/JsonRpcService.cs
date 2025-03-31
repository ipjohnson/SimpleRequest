using DependencyModules.Runtime.Attributes;
using SimpleRequest.JsonRpc.Impl;

namespace SimpleRequest.JsonRpc;

[DependencyModule(OnlyRealm = true)]
[JsonRpcImplModule]
public partial class JsonRpcService {
    public string? Path { get; set; }
    
    public string[] Tags { get; set; } = [];
}
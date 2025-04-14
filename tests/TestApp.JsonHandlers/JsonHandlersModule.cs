using DependencyModules.Runtime.Attributes;
using SimpleRequest.JsonRpc;

namespace TestApp.JsonHandlers;

[DependencyModule]
[JsonRpcService(Path = "/json-rpc")]
public partial class JsonHandlersModule;
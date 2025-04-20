using DependencyModules.Runtime.Attributes;
using SimpleRequest.Caching;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Swagger;

[DependencyModule(GenerateFactories = true)]
[SimpleRequestRuntime]
[RoutingOrder(1000)]
[SimpleRequestCaching]
public partial class SimpleRequestSwagger;
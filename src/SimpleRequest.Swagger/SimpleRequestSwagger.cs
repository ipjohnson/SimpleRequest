using DependencyModules.Runtime.Attributes;
using SimpleRequest.Caching;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.Swagger;

[DependencyModule(GenerateFactories = true)]
[SimpleRequestWeb]
[RoutingOrder(1000)]
[SimpleRequestCaching]
public partial class SimpleRequestSwagger;
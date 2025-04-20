using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.SwaggerUi;

[DependencyModule(GenerateFactories = true)]
[SimpleRequestRuntime]
[RoutingOrder(1001)]
public partial class SimpleRequestSwaggerUi;
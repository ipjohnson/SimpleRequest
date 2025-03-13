using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.SwaggerUi;

[DependencyModule]
[SimpleRequestWeb]
[RoutingOrder(1001)]
public partial class SimpleRequestSwaggerUi { }
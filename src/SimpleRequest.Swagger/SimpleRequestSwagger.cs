using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.Swagger;

[DependencyModule]
[SimpleRequestWeb.Attribute]
[RoutingOrder(1000)]
public partial class SimpleRequestSwagger {
    
}
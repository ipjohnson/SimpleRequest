using DependencyModules.Runtime.Attributes;
using SimpleRequest.Caching;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.Swagger;

[DependencyModule]
[SimpleRequestWeb.Attribute]
[RoutingOrder(1000)]
[SimpleRequestCaching.Attribute]
public partial class SimpleRequestSwagger {
    
}
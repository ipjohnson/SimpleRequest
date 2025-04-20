using DependencyModules.Runtime.Attributes;
using SimpleRequest.RazorBlade;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace TestApp.WebHandlers;

[DependencyModule]
[SimpleRequestRuntime]
[RoutingOrder(-1)]
[RazorBladeRuntime]
[StaticContent(RequestPath = "/static-content/")]
public partial class TestWebHandlers;
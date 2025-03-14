using DependencyModules.Runtime.Attributes;
using SimpleRequest.RazorBlade;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace TestApp.WebHandlers;

[DependencyModule]
[SimpleRequestWeb]
[RoutingOrder(-1)]
[RazorBladeRuntime]
public partial class TestWebHandlers {
    
}
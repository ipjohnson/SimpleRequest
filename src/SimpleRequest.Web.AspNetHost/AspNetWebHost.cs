using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.Web.AspNetHost;

[DependencyModule]
[SimpleRequestWeb.Attribute]
public partial class AspNetWebHost {
    public partial class Attribute : ISimpleRequestEntryAttribute {
        
    }
}
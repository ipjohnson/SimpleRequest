using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Web.Runtime;

[DependencyModule]
[SimpleRequestRuntime.Attribute]
public partial class SimpleRequestWeb {
    public partial class Attribute : ISimpleRequestEntryAttribute {
        
    }
}
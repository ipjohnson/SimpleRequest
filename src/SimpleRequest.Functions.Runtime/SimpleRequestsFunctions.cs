using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Functions.Runtime;

[DependencyModule]
[SimpleRequestRuntime.Attribute]
public partial class SimpleRequestFunctions {
    public partial class Attribute : ISimpleRequestEntryAttribute {
        
    }
}
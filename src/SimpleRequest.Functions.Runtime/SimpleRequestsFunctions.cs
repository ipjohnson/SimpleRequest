using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;

namespace SimpleRequest.Functions.Runtime;

[DependencyModule]
[SimpleRequestRuntime.Attribute]
public partial class SimpleRequestFunctions {
    
    public partial class Attribute : ISimpleRequestFunctionsAttribute {
        
    }
    
    public interface ISimpleRequestFunctionsAttribute {
        
    }
}
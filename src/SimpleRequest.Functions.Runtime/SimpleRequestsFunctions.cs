using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Functions.Runtime;

public partial class SimpleRequestFunctionsAttribute : ISimpleRequestEntryAttribute;

[DependencyModule]
[SimpleRequestRuntime]
public partial class SimpleRequestFunctions;
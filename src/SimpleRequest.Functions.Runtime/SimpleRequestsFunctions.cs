using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Functions.Runtime;

public partial class SimpleRequestFunctionsAttribute : ISimpleRequestEntryAttribute;

[DependencyModule(GenerateFactories = true)]
[SimpleRequestRuntime]
public partial class SimpleRequestFunctions;
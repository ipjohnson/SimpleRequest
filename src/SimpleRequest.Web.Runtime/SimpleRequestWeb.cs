using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Web.Runtime;


public partial class SimpleRequestWebAttribute : ISimpleRequestEntryAttribute;

[DependencyModule]
[SimpleRequestRuntime]
public partial class SimpleRequestWeb;
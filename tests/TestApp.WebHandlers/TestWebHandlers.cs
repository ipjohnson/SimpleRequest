﻿using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace TestApp.WebHandlers;

[DependencyModule]
[SimpleRequestWeb.Attribute]
[RoutingOrder(-1)]
public partial class TestWebHandlers {
    
}
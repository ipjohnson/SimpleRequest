using DependencyModules.Runtime;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.Web.AspNetHost;

[DependencyModule]
[SimpleRequestWeb]
public partial class AspNetWebHost {
    public partial class Attribute : ISimpleRequestEntryAttribute { }

    public static void Run<T>(bool useAspNetRouting = true, bool slim = false, string[]? args = null) 
        where T : IDependencyModule, new() {
        Run(useAspNetRouting, slim, args ?? Array.Empty<string>(), new T());
    }

    public static void Run(bool useAspNetRouting, bool slim, string[] args, params IDependencyModule[] modules) {
        WebApplicationBuilder builder;

        if (slim) {
            builder = WebApplication.CreateSlimBuilder(args);
        }
        else {
            builder = WebApplication.CreateBuilder(args);
        }
        
        builder.Services.AddModules(modules);
        
        var app = builder.Build();

        foreach (var module in modules) {
            if (module is IWebApplicationConfiguration configuration) {
                configuration.Configure(app);
            }
        }

        app.UseSimpleRequest(useAspNetRouting);
        
        app.Run();
    }
}
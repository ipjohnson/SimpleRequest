using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Swagger;
using SimpleRequest.SwaggerUi;
using SimpleRequest.Web.AspNetHost;
using TestApp.WebHandlers;

namespace TestApp.AspNet;

[DependencyModule]
[TestWebHandlers.Attribute]
[AspNetWebHost.Attribute]
[SimpleRequestSwagger.Attribute]
[SimpleRequestSwaggerUi.Attribute]
public partial class TestWebApp :IServiceCollectionConfiguration {

    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<IMetricLoggerProvider, NullMetricLoggerProvider>();
    }
}
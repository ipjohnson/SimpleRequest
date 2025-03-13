using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Swagger;
using SimpleRequest.SwaggerUi;
using SimpleRequest.Web.AspNetHost;
using TestApp.WebHandlers;

namespace TestApp.AspNet;

[DependencyModule]
[TestWebHandlers]
[AspNetWebHost]
[SimpleRequestSwagger]
[SimpleRequestSwaggerUi]
public partial class Application :IServiceCollectionConfiguration {

    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<IMetricLoggerProvider, NullMetricLoggerProvider>();
    }
}
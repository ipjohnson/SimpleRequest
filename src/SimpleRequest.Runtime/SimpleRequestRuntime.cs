using CompiledTemplateEngine.Runtime;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Runtime;

[DependencyModule]
[CompiledTemplateEngineRuntime.Attribute]
public partial class SimpleRequestRuntime : IServiceCollectionConfiguration {

    public void ConfigureServices(IServiceCollection services) {
        services.AddLogging(LoggerActionHelper.CreateAction());
        services.RemoveAll<ILoggerProvider>();
    }
}
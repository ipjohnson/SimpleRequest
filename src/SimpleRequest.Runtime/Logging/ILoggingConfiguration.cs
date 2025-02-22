using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleRequest.Runtime.Logging;

public interface ILoggingConfigurationImplementation {
    void Configure(IServiceCollection services, IEnumerable<ILoggingBuilderConfiguration> configurations);
}

public interface ILoggingBuilderConfiguration {
    void Configure(ILoggingBuilder builder);
}
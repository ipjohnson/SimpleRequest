using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace SimpleRequest.Runtime.Logging;

public static class DefaultLoggingHelper {

    public static void Configure(IServiceCollection services, IEnumerable<ILoggingBuilderConfiguration> configurations) {
        var logLevel = GetLogLevel();
        services.AddLogging(builder => SetupLogging(builder, configurations, logLevel));
        services.RemoveAll<ILoggerProvider>();
    }

    private static void SetupLogging(
        ILoggingBuilder builder, IEnumerable<ILoggingBuilderConfiguration> configurations, LogLevel logLevel) {

        foreach (var configuration in configurations) {
            configuration.Configure(builder);
        }
        
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("SimpleRequest", LogLevel.Information)
            .SetMinimumLevel(logLevel);
    }

    private static LogLevel GetLogLevel() {
        var logLevel = LogLevel.Information;
        
        var logLevelString = 
            Environment.GetEnvironmentVariable("LOG_LEVEL");

        if (!string.IsNullOrEmpty(logLevelString) && 
            Enum.TryParse(typeof(LogLevel), logLevelString, out var newLogLevel)) {
            logLevel = (LogLevel)newLogLevel;
        }

        return logLevel;
    }
}
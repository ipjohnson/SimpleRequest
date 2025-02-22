using Microsoft.Extensions.Logging;

namespace SimpleRequest.Runtime.Logging;

public class LoggerActionHelper {
    
    private readonly LogLevel _logLevel;

    public LoggerActionHelper(LogLevel logLevel) {
        _logLevel = logLevel;
    }

    public void BuildLogger(ILoggingBuilder builder) {
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("SimpleRequest", LogLevel.Information)
            .SetMinimumLevel(_logLevel);
    }

    public static Action<ILoggingBuilder> CreateAction() {
        LogLevel logLevel = LogLevel.Information;
        
        var logLevelString = 
            Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information";

            if (Enum.TryParse(typeof(LogLevel), logLevelString, out var newLogLevel)) {
                logLevel = (LogLevel)newLogLevel!;
            }
        

        return new LoggerActionHelper(logLevel).BuildLogger;
    }
}
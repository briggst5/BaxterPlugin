using Microsoft.Extensions.Logging;

namespace PolarionMcp.Server;

public static class ServerLogging
{
    public static LogLevel ParseLogLevel(string? rawLogLevel)
    {
        if (string.IsNullOrWhiteSpace(rawLogLevel))
        {
            return LogLevel.Warning;
        }

        return rawLogLevel.Trim().ToUpperInvariant() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" => LogLevel.Debug,
            "INFORMATION" or "INFO" => LogLevel.Information,
            "WARNING" or "WARN" => LogLevel.Warning,
            "ERROR" => LogLevel.Error,
            "CRITICAL" or "FATAL" => LogLevel.Critical,
            "NONE" or "OFF" => LogLevel.None,
            _ => throw new InvalidOperationException(
                "POLARION_MCP_LOG_LEVEL must be one of TRACE, DEBUG, INFO, WARNING, ERROR, CRITICAL, or NONE."
            )
        };
    }
}

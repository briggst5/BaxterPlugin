using System.Globalization;

namespace PolarionMcp.Client;

public sealed class PolarionClientOptions
{
    public const string SectionName = "POLARION";

    public string Url { get; init; } = string.Empty;
    public string User { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string PersonalAccessToken { get; init; } = string.Empty;
    public string Project { get; init; } = string.Empty;
    public string LogLevel { get; init; } = "WARNING";
    public string TlsCaFile { get; init; } = string.Empty;
    public bool TlsSkipVerify { get; init; } = true;
    public int SessionMaxRetries { get; init; } = 1;
    public int SessionIdleRefreshSeconds { get; init; } = 0;
    public int? MaxCalls { get; init; }

    public static PolarionClientOptions FromEnvironment()
    {
        var maxCallsRaw = Environment.GetEnvironmentVariable("POLARION_MCP_MAX_CALLS");
        int? maxCalls = null;
        if (!string.IsNullOrWhiteSpace(maxCallsRaw))
        {
            if (!int.TryParse(maxCallsRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedMaxCalls))
            {
                throw new InvalidOperationException("POLARION_MCP_MAX_CALLS must be an integer.");
            }

            if (parsedMaxCalls < 0)
            {
                throw new InvalidOperationException("POLARION_MCP_MAX_CALLS must be >= 0.");
            }

            maxCalls = parsedMaxCalls;
        }

        return new PolarionClientOptions
        {
            Url = (Environment.GetEnvironmentVariable("POLARION_URL") ?? string.Empty).Trim(),
            User = (Environment.GetEnvironmentVariable("POLARION_USER") ?? string.Empty).Trim(),
            Password = (Environment.GetEnvironmentVariable("POLARION_PASSWORD") ?? string.Empty).Trim(),
            PersonalAccessToken = (Environment.GetEnvironmentVariable("POLARION_PAT") ?? string.Empty).Trim(),
            Project = (Environment.GetEnvironmentVariable("POLARION_PROJECT") ?? string.Empty).Trim(),
            LogLevel = (Environment.GetEnvironmentVariable("POLARION_MCP_LOG_LEVEL") ?? "WARNING").Trim(),
            TlsCaFile = (Environment.GetEnvironmentVariable("POLARION_TLS_CA_FILE") ?? string.Empty).Trim(),
            TlsSkipVerify = ParseBooleanEnvironmentVariable("POLARION_TLS_SKIP_VERIFY", defaultValue: true),
            SessionMaxRetries = ParseIntegerEnvironmentVariable("POLARION_SESSION_MAX_RETRIES", defaultValue: 1, minValue: 0),
            SessionIdleRefreshSeconds = ParseIntegerEnvironmentVariable("POLARION_SESSION_IDLE_REFRESH_SECONDS", defaultValue: 0, minValue: 0),
            MaxCalls = maxCalls
        };
    }

    private static bool ParseBooleanEnvironmentVariable(string key, bool defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "on" => true,
            "0" or "false" or "no" or "off" => false,
            _ => throw new InvalidOperationException($"{key} must be a boolean (true/false, 1/0, yes/no).")
        };
    }

    private static int ParseIntegerEnvironmentVariable(string key, int defaultValue, int minValue)
    {
        var raw = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"{key} must be an integer.");
        }

        if (parsed < minValue)
        {
            throw new InvalidOperationException($"{key} must be >= {minValue.ToString(CultureInfo.InvariantCulture)}.");
        }

        return parsed;
    }
}

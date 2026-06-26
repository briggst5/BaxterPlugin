namespace GqpMcp.Core;

public sealed class GqpOptions
{
    public const string DefaultSearchEndpoint = "https://baxter-platform-docs.search.windows.net";
    public const string DefaultEmbedEndpoint = "https://aoi-flc-copilot-platform.openai.azure.com/";
    public const string DefaultGptEndpoint = "https://aoi-flc-copilot-platform.openai.azure.com/";
    public const string DefaultApiVersion = "2024-02-01";
    public const string IndexName = "gqp-documents";
    public const string EmbeddingDeployment = "text-embedding-3-large";
    public const string GptDeployment = "gpt-4.1";
    public const int DefaultTopK = 8;

    public string SearchEndpoint { get; init; } = DefaultSearchEndpoint;
    public string EmbedEndpoint { get; init; } = DefaultEmbedEndpoint;
    public string GptEndpoint { get; init; } = DefaultGptEndpoint;
    public string ApiVersion { get; init; } = DefaultApiVersion;
    public string? KeyVaultName { get; init; }
    public string? AzureTenantId { get; init; }
    public string? SearchKey { get; init; }
    public string? OpenAiKey { get; init; }
    public string? GptKey { get; init; }
    public string LogLevel { get; init; } = "Information";
    public string TlsCaFile { get; init; } = string.Empty;
    public bool TlsSkipVerify { get; init; }

    public static GqpOptions FromEnvironment()
    {
        return new GqpOptions
        {
            SearchEndpoint = GetEnv("AZURE_SEARCH_ENDPOINT") ?? DefaultSearchEndpoint,
            EmbedEndpoint = GetEnv("AZURE_OPENAI_ENDPOINT") ?? DefaultEmbedEndpoint,
            GptEndpoint = GetEnv("AZURE_GPT_ENDPOINT") ?? DefaultGptEndpoint,
            ApiVersion = GetEnv("AZURE_OPENAI_API_VERSION") ?? DefaultApiVersion,
            KeyVaultName = GetEnv("GQP_KEYVAULT_NAME"),
            AzureTenantId = GetEnv("AZURE_TENANT_ID"),
            SearchKey = GetEnv("AZURE_SEARCH_KEY"),
            OpenAiKey = GetEnv("AZURE_OPENAI_KEY"),
            GptKey = GetEnv("AZURE_GPT_KEY") ?? GetEnv("AZURE_OPENAI_KEY"),
            LogLevel = GetEnv("GQP_MCP_LOG_LEVEL") ?? "Information",
            TlsCaFile = (GetEnv("GQP_TLS_CA_FILE") ?? GetEnv("SSL_CERT_FILE") ?? string.Empty).Trim(),
            TlsSkipVerify = ParseBooleanEnv("GQP_TLS_SKIP_VERIFY", defaultValue: false),
        };
    }

    private static string? GetEnv(string key) =>
        Environment.GetEnvironmentVariable(key) is { Length: > 0 } value ? value : null;

    private static bool ParseBooleanEnv(string key, bool defaultValue)
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
            _ => throw new InvalidOperationException($"{key} must be true/false, 1/0, yes/no, or on/off."),
        };
    }
}

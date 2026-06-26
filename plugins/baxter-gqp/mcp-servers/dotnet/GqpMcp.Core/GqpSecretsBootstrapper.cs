using System.Text.Json;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace GqpMcp.Core;

public static class GqpSecretsBootstrapper
{
    private static readonly IReadOnlyDictionary<string, string> SecretMap =
        new Dictionary<string, string>
        {
            ["gqp-azure-search-key"] = "AZURE_SEARCH_KEY",
            ["gqp-azure-openai-key"] = "AZURE_OPENAI_KEY",
        };

    /// <summary>
    /// Environment variables that, once populated, are sufficient to call the backend
    /// without any Entra sign-in. These are what we cache locally after the first fetch.
    /// </summary>
    private static readonly string[] CachedEnvVars = ["AZURE_SEARCH_KEY", "AZURE_OPENAI_KEY"];

    private static string SecretCachePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "gqp-mcp.secrets.json");

    /// <summary>
    /// Ensures backend API keys are available in the environment. Uses the locally cached
    /// keys when present (no network, no Entra); otherwise reads them from Key Vault using
    /// the supplied Entra credential and caches them so future runs never sign in again.
    /// </summary>
    public static async Task EnsureSecretsAsync(
        GqpOptions options,
        TokenCredential credential,
        ILogger? logger,
        CancellationToken cancellationToken,
        bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(options.KeyVaultName))
        {
            return;
        }

        if (!forceRefresh && TryLoadCachedSecretsIntoEnvironment())
        {
            logger?.LogInformation("Loaded GQP API keys from local cache (no Entra sign-in needed)");
            return;
        }

        if (forceRefresh)
        {
            ClearCachedSecrets(logger);
        }

        await LoadKeyVaultSecretsAsync(options, credential, logger, cancellationToken);
    }

    /// <summary>
    /// Deletes the local secret cache and clears the in-process key environment variables so
    /// the next access re-reads from Key Vault (and re-authenticates if the Entra token is
    /// also gone). Used to recover from rotated/invalid keys.
    /// </summary>
    public static void ClearCachedSecrets(ILogger? logger = null)
    {
        foreach (var envVar in CachedEnvVars)
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }

        Environment.SetEnvironmentVariable("AZURE_GPT_KEY", null);

        try
        {
            if (File.Exists(SecretCachePath))
            {
                File.Delete(SecretCachePath);
                logger?.LogInformation("Cleared local GQP secret cache; will re-fetch from Key Vault");
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not delete local GQP secret cache");
        }
    }

    public static async Task LoadKeyVaultSecretsAsync(
        GqpOptions options,
        TokenCredential credential,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.KeyVaultName))
        {
            return;
        }

        var vaultUrl = $"https://{options.KeyVaultName.TrimEnd('/')}.vault.azure.net/";
        var clientOptions = new SecretClientOptions
        {
            Transport = GqpHttpTransport.CreateAzureTransport(options),
        };
        var client = new SecretClient(new Uri(vaultUrl), credential, clientOptions);

        foreach (var (secretName, envVar) in SecretMap)
        {
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            Environment.SetEnvironmentVariable(envVar, response.Value.Value);
        }

        Environment.SetEnvironmentVariable("AZURE_GPT_KEY", Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"));
        SaveSecretsCache(logger);
        logger?.LogInformation("Loaded GQP API keys from Key Vault {VaultName}", options.KeyVaultName);
    }

    /// <summary>
    /// True when a usable local secret cache exists, meaning the MCP can run without any
    /// Entra sign-in.
    /// </summary>
    public static bool HasCachedSecrets(GqpOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.KeyVaultName))
        {
            return false;
        }

        return ReadCache() is { } cache && CachedEnvVars.All(cache.ContainsKey);
    }

    private static bool TryLoadCachedSecretsIntoEnvironment()
    {
        var cache = ReadCache();
        if (cache is null || !CachedEnvVars.All(cache.ContainsKey))
        {
            return false;
        }

        foreach (var (key, value) in cache)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        if (Environment.GetEnvironmentVariable("AZURE_GPT_KEY") is null or { Length: 0 })
        {
            Environment.SetEnvironmentVariable("AZURE_GPT_KEY", Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"));
        }

        return true;
    }

    private static Dictionary<string, string>? ReadCache()
    {
        try
        {
            if (!File.Exists(SecretCachePath))
            {
                return null;
            }

            var json = File.ReadAllText(SecretCachePath);
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return values is { Count: > 0 } ? values : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void SaveSecretsCache(ILogger? logger)
    {
        try
        {
            var values = new Dictionary<string, string>();
            foreach (var envVar in CachedEnvVars)
            {
                var value = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(value))
                {
                    values[envVar] = value;
                }
            }

            if (values.Count == 0)
            {
                return;
            }

            var directory = Path.GetDirectoryName(SecretCachePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(SecretCachePath, JsonSerializer.Serialize(values));
            TrySetOwnerOnlyPermissions(SecretCachePath);
        }
        catch (Exception ex)
        {
            // Non-fatal: caching is an optimization. The MCP still works, it will just
            // re-fetch from Key Vault next time.
            logger?.LogWarning(ex, "Could not cache GQP secrets locally");
        }
    }

    private static void TrySetOwnerOnlyPermissions(string path)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
        catch (Exception)
        {
            // Best effort; non-fatal if the platform rejects chmod.
        }
    }
}

using Azure.Core;
using Azure.Identity;

namespace GqpMcp.Core;

/// <summary>
/// Builds the Entra credential used to read Key Vault (and, when no Key Vault is configured,
/// the Search/OpenAI data planes directly).
///
/// Mirrors the proven CFCT pattern: <see cref="DefaultAzureCredential"/> with interactive
/// credentials enabled. This uses the user's ambient Entra identity - Windows WAM/SSO, Visual
/// Studio, or the Azure CLI - and only falls back to an interactive browser via Microsoft's
/// pre-consented client. There is deliberately no custom app registration, client secret, or
/// per-user consent: access is governed entirely by the caller's Azure RBAC on the resources.
/// </summary>
public static class GqpCredentialFactory
{
    private const string VaultScope = "https://vault.azure.net/.default";
    private const string CognitiveServicesScope = "https://cognitiveservices.azure.com/.default";
    private const string SearchScope = "https://search.azure.com/.default";

    /// <summary>
    /// Runtime credential for the MCP server. Interactive is enabled so that, on a machine with
    /// no ambient login, a one-time browser sign-in can still unlock Key Vault.
    /// </summary>
    public static TokenCredential CreateRuntimeTokenCredential(GqpOptions options) =>
        CreateDefaultCredential(options, includeInteractive: true);

    /// <summary>
    /// Credential for the explicit `authenticate` subcommand. Same as runtime; the
    /// <paramref name="useDeviceCode"/> flag is accepted for CLI compatibility but ignored,
    /// since DefaultAzureCredential selects the best available method automatically.
    /// </summary>
    public static TokenCredential CreateInteractiveTokenCredential(GqpOptions options, bool useDeviceCode = false) =>
        CreateDefaultCredential(options, includeInteractive: true);

    private static TokenCredential CreateDefaultCredential(GqpOptions options, bool includeInteractive)
    {
        // Silent/ambient providers: Azure CLI, Windows WAM/SSO, Visual Studio, shared token
        // cache. These cover the common cases (CFCT relies on exactly these) and are safe on
        // WSL/Linux. Interactive browser is excluded here because DefaultAzureCredential's
        // built-in browser tries to persist to the OS keyring (libsecret), which is absent on
        // Baxter WSL and throws "Persistence check failed".
        var silentOptions = new DefaultAzureCredentialOptions
        {
            ExcludeManagedIdentityCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            Transport = GqpHttpTransport.CreateAzureTransport(options),
        };

        if (!string.IsNullOrWhiteSpace(options.AzureTenantId))
        {
            silentOptions.TenantId = options.AzureTenantId;
            silentOptions.SharedTokenCacheTenantId = options.AzureTenantId;
            silentOptions.VisualStudioTenantId = options.AzureTenantId;
        }

        var silent = new DefaultAzureCredential(silentOptions);

        if (!includeInteractive)
        {
            return silent;
        }

        // Interactive fallback for a machine with no ambient login. No custom ClientId, so it
        // uses Microsoft's pre-consented client (no custom-app admin-consent wall), and an
        // in-memory token cache (no libsecret dependency). Re-prompting is a non-issue because
        // the fetched Key Vault keys are cached locally after the first success.
        var browserOptions = new InteractiveBrowserCredentialOptions
        {
            Transport = GqpHttpTransport.CreateAzureTransport(options),
        };

        if (!string.IsNullOrWhiteSpace(options.AzureTenantId))
        {
            browserOptions.TenantId = options.AzureTenantId;
        }

        return new ChainedTokenCredential(silent, new InteractiveBrowserCredential(browserOptions));
    }

    /// <summary>
    /// Silent check used by `check-auth`: never prompts. Returns true if the backend keys are
    /// already cached locally, or if a token can be acquired without interaction.
    /// </summary>
    public static async Task<bool> HasValidTokenAsync(GqpOptions options, CancellationToken cancellationToken)
    {
        // Once the backend keys are cached locally, no Entra token is needed at all.
        if (GqpSecretsBootstrapper.HasCachedSecrets(options))
        {
            return true;
        }

        try
        {
            var credential = CreateDefaultCredential(options, includeInteractive: false);
            foreach (var scope in GetWarmUpScopes(options))
            {
                await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Acquires a token (interactively if needed) and warms up the required scopes. With
    /// DefaultAzureCredential there is no AuthenticationRecord to persist - the underlying
    /// credential providers (WAM, az, MSAL cache) handle their own token persistence.
    /// </summary>
    public static async Task AuthenticateAndPersistAsync(
        GqpOptions options,
        bool useDeviceCode,
        CancellationToken cancellationToken)
    {
        var credential = CreateDefaultCredential(options, includeInteractive: true);
        await WarmUpAsync(options, credential, cancellationToken);
    }

    public static async Task WarmUpAsync(GqpOptions options, TokenCredential credential, CancellationToken cancellationToken)
    {
        foreach (var scope in GetWarmUpScopes(options))
        {
            await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
        }
    }

    public static IReadOnlyList<string> GetWarmUpScopes(GqpOptions options)
    {
        // Key Vault mode: Entra is only used to read the backend API keys from Key Vault.
        // The keys (not Entra) then authenticate to Search/OpenAI, so the vault scope is all
        // that is ever requested.
        if (!string.IsNullOrWhiteSpace(options.KeyVaultName))
        {
            return new[] { VaultScope };
        }

        // No Key Vault: fall back to direct Entra RBAC against the data planes.
        return new[] { CognitiveServicesScope, SearchScope };
    }
}

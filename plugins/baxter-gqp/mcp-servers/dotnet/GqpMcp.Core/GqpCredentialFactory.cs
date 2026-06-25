using Azure.Core;
using Azure.Identity;

namespace GqpMcp.Core;

public static class GqpCredentialFactory
{
    private const string VaultScope = "https://vault.azure.net/.default";
    private const string CognitiveServicesScope = "https://cognitiveservices.azure.com/.default";
    private const string SearchScope = "https://search.azure.com/.default";

    /// <summary>
    /// For MCP stdio runtime ù Azure CLI, IDE login, then browser cache as last resort.
    /// </summary>
    public static TokenCredential CreateRuntimeTokenCredential(GqpOptions options)
    {
        var chain = new List<TokenCredential>
        {
            new AzureCliCredential(),
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeManagedIdentityCredential = true,
                ExcludeInteractiveBrowserCredential = true,
            }),
        };

        if (ShouldIncludeBrowserCredential(options))
        {
            chain.Add(CreateBrowserCredential(options));
        }

        return new ChainedTokenCredential(chain.ToArray());
    }

    /// <summary>
    /// For `gqp-mcp authenticate` ù Azure CLI, device-code, DAC, then browser last.
    /// </summary>
    public static TokenCredential CreateInteractiveTokenCredential(GqpOptions options, bool useDeviceCode = false)
    {
        var mode = ResolveAuthMode(options, useDeviceCode);
        var chain = new List<TokenCredential> { new AzureCliCredential() };

        if (mode is GqpAuthMode.DeviceCode or GqpAuthMode.Auto)
        {
            chain.Add(CreateDeviceCodeCredential(options));
        }

        chain.Add(new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeManagedIdentityCredential = true,
            ExcludeInteractiveBrowserCredential = true,
        }));

        if (mode is GqpAuthMode.Browser or GqpAuthMode.Auto)
        {
            chain.Add(CreateBrowserCredential(options));
        }

        return new ChainedTokenCredential(chain.ToArray());
    }

    public static async Task<bool> HasValidTokenAsync(
        GqpOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var credential = CreateRuntimeTokenCredential(options);
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

    public static async Task WarmUpAsync(GqpOptions options, TokenCredential credential, CancellationToken cancellationToken)
    {
        foreach (var scope in GetWarmUpScopes(options))
        {
            await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
        }
    }

    public static IReadOnlyList<string> GetWarmUpScopes(GqpOptions options)
    {
        var scopes = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.KeyVaultName))
        {
            scopes.Add(VaultScope);
        }

        scopes.Add(CognitiveServicesScope);
        scopes.Add(SearchScope);
        return scopes;
    }

    public static GqpAuthMode ResolveAuthMode(GqpOptions options, bool deviceCodeFlag)
    {
        if (deviceCodeFlag)
        {
            return GqpAuthMode.DeviceCode;
        }

        return options.AuthMode switch
        {
            GqpAuthMode.Browser => GqpAuthMode.Browser,
            GqpAuthMode.DeviceCode => GqpAuthMode.DeviceCode,
            _ when string.IsNullOrWhiteSpace(options.AzureClientId) => GqpAuthMode.DeviceCode,
            _ => GqpAuthMode.Auto,
        };
    }

    private static bool ShouldIncludeBrowserCredential(GqpOptions options) =>
        options.AuthMode is GqpAuthMode.Browser or GqpAuthMode.Auto
        && !string.IsNullOrWhiteSpace(options.AzureClientId);

    private static DeviceCodeCredential CreateDeviceCodeCredential(GqpOptions options)
    {
        var deviceOptions = new DeviceCodeCredentialOptions
        {
            TenantId = options.AzureTenantId,
            DeviceCodeCallback = (code, cancellation) =>
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("GQP MCP: Baxter sign-in required.");
                Console.Error.WriteLine($"  Open {code.VerificationUri} in your browser");
                Console.Error.WriteLine($"  Enter code: {code.UserCode}");
                Console.Error.WriteLine("  (Also visible in Cursor Settings ? MCP ? gqp-knowledge logs)");
                Console.Error.WriteLine();
                return Task.CompletedTask;
            },
        };

        if (!string.IsNullOrWhiteSpace(options.AzureClientId))
        {
            deviceOptions.ClientId = options.AzureClientId;
        }

        return new DeviceCodeCredential(deviceOptions);
    }

    private static InteractiveBrowserCredential CreateBrowserCredential(GqpOptions options)
    {
        var browserOptions = new InteractiveBrowserCredentialOptions
        {
            TenantId = options.AzureTenantId,
            ClientId = options.AzureClientId,
            RedirectUri = new Uri(options.AzureRedirectUri),
        };

        return new InteractiveBrowserCredential(browserOptions);
    }
}

public enum GqpAuthMode
{
    Auto,
    Browser,
    DeviceCode,
}

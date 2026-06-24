using Azure.Core;
using Azure.Identity;

namespace GqpMcp.Core;

public static class GqpCredentialFactory
{
    private static readonly string[] WarmUpScopes =
    [
        "https://vault.azure.net/.default",
        "https://cognitiveservices.azure.com/.default",
        "https://search.azure.com/.default",
    ];

    private const string CheckAuthScope = "https://vault.azure.net/.default";

    /// <summary>
    /// For MCP stdio runtime — uses cached tokens, Azure CLI, IDE login, then silent browser cache.
    /// Browser opens only when no cached Entra token exists (e.g. after expiry).
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
            CreateBrowserCredential(options),
        };

        return new ChainedTokenCredential(chain.ToArray());
    }

    /// <summary>
    /// For `gqp-mcp authenticate` — browser or device-code login.
    /// </summary>
    public static TokenCredential CreateInteractiveTokenCredential(GqpOptions options, bool useDeviceCode = false)
    {
        var chain = new List<TokenCredential> { CreateBrowserCredential(options) };

        if (useDeviceCode)
        {
            chain.Add(new DeviceCodeCredential(new DeviceCodeCredentialOptions
            {
                TenantId = options.AzureTenantId,
                ClientId = options.AzureClientId,
            }));
        }

        chain.Add(new AzureCliCredential());
        chain.Add(new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeManagedIdentityCredential = true,
        }));

        return new ChainedTokenCredential(chain.ToArray());
    }

    public static async Task<bool> HasValidTokenAsync(
        GqpOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var credential = CreateRuntimeTokenCredential(options);
            await credential.GetTokenAsync(new TokenRequestContext([CheckAuthScope]), cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task WarmUpAsync(TokenCredential credential, CancellationToken cancellationToken)
    {
        foreach (var scope in WarmUpScopes)
        {
            await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
        }
    }

    private static InteractiveBrowserCredential CreateBrowserCredential(GqpOptions options)
    {
        var browserOptions = new InteractiveBrowserCredentialOptions
        {
            TenantId = options.AzureTenantId,
            ClientId = options.AzureClientId,
            RedirectUri = new Uri("http://localhost"),
        };

        return new InteractiveBrowserCredential(browserOptions);
    }
}

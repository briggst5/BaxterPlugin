using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
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
        logger?.LogInformation("Loaded GQP API keys from Key Vault {VaultName}", options.KeyVaultName);
    }
}

using GqpMcp.Core;
using Microsoft.Extensions.Logging;

namespace GqpMcp.Server;

public static class GqpStartup
{
    public static GqpOptions LoadOptions()
    {
        GqpEnvFileLoader.LoadEnvironmentVariables();
        return GqpOptions.FromEnvironment();
    }

    public static async Task<int> RunCheckAuthAsync()
    {
        var options = LoadOptions();

        if (await GqpCredentialFactory.HasValidTokenAsync(options, CancellationToken.None))
        {
            return 0;
        }

        var hint = string.IsNullOrWhiteSpace(options.KeyVaultName)
            ? "Entra sign-in required."
            : "Entra sign-in required to read Key Vault secrets.";
        Console.Error.WriteLine($"GQP MCP: no valid Entra token found. {hint}");
        Console.Error.WriteLine("  Run: gqp-mcp authenticate");
        Console.Error.WriteLine("  (Uses your Windows/Entra sign-in or Azure CLI; opens a browser only if needed.)");
        return 1;
    }

    public static async Task<int> RunAuthenticateAsync(string[] args)
    {
        var useDeviceCode = args.Contains("--device-code", StringComparer.OrdinalIgnoreCase);

        var options = LoadOptions();

        Console.Error.WriteLine($"INFO: {GqpHttpTransport.DescribeTlsConfig(options)}");

        try
        {
            if (await GqpCredentialFactory.HasValidTokenAsync(options, CancellationToken.None))
            {
                Console.Error.WriteLine("GQP MCP: already authenticated (cached token reused; no sign-in needed).");
            }
            else
            {
                await GqpCredentialFactory.AuthenticateAndPersistAsync(options, useDeviceCode, CancellationToken.None);
            }

            if (!string.IsNullOrWhiteSpace(options.KeyVaultName))
            {
                var credential = GqpCredentialFactory.CreateRuntimeTokenCredential(options);
                await GqpSecretsBootstrapper.EnsureSecretsAsync(
                    options,
                    credential,
                    logger: null,
                    CancellationToken.None);
                Console.Error.WriteLine(
                    "INFO: backend API keys cached locally; future sessions need no sign-in.");
            }
            else
            {
                Console.Error.WriteLine(
                    "INFO: GQP_KEYVAULT_NAME unset; using Entra RBAC for Azure Search and OpenAI.");
            }

            Console.Error.WriteLine(
                "GQP MCP authentication successful. Sign-in persisted; future sessions are silent until token expiry.");
            return 0;
        }
        catch (Exception ex)
        {
            if (GqpHttpTransport.IsCertificateError(ex))
            {
                Console.Error.WriteLine(GqpHttpTransport.CertificateErrorHelp(options));
            }
            else
            {
                Console.Error.WriteLine($"GQP MCP authentication failed: {ex.Message}");
            }

            return 1;
        }
    }
}

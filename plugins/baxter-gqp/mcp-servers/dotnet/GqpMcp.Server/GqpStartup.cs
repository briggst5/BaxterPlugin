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
            ? "Entra sign-in required (device-code or az login)."
            : "Entra sign-in required to read Key Vault secrets (device-code or az login).";
        Console.Error.WriteLine($"GQP MCP: no valid Entra token found. {hint}");
        Console.Error.WriteLine("  Run: gqp-mcp authenticate --device-code");
        Console.Error.WriteLine("  Or:  az login");
        return 1;
    }

    public static async Task<int> RunAuthenticateAsync(string[] args)
    {
        var useDeviceCode = args.Contains("--device-code", StringComparer.OrdinalIgnoreCase);
        var useBrowser = args.Contains("--browser", StringComparer.OrdinalIgnoreCase);

        var options = LoadOptions();
        if (useBrowser)
        {
            Environment.SetEnvironmentVariable("GQP_AUTH_MODE", "browser");
            options = GqpOptions.FromEnvironment();
        }

        var authMode = GqpCredentialFactory.ResolveAuthMode(options, useDeviceCode);
        Console.Error.WriteLine($"INFO: {GqpHttpTransport.DescribeTlsConfig(options)}");
        Console.Error.WriteLine($"INFO: auth mode: {authMode}");

        var credential = GqpCredentialFactory.CreateInteractiveTokenCredential(options, useDeviceCode: useDeviceCode);

        try
        {
            await GqpCredentialFactory.WarmUpAsync(options, credential, CancellationToken.None);

            if (!string.IsNullOrWhiteSpace(options.KeyVaultName))
            {
                await GqpSecretsBootstrapper.LoadKeyVaultSecretsAsync(
                    options,
                    credential,
                    logger: null,
                    CancellationToken.None);
            }
            else
            {
                Console.Error.WriteLine(
                    "INFO: GQP_KEYVAULT_NAME unset; using Entra RBAC for Azure Search and OpenAI.");
            }

            Console.Error.WriteLine("GQP MCP authentication successful. Token cached for Azure services.");
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

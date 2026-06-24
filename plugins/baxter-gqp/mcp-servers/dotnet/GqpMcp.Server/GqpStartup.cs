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

        Console.Error.WriteLine("GQP MCP: no valid Entra token found. Browser sign-in required.");
        return 1;
    }

    public static async Task<int> RunAuthenticateAsync(string[] args)
    {
        var useDeviceCode = args.Contains("--device-code", StringComparer.OrdinalIgnoreCase);

        var options = LoadOptions();
        Console.Error.WriteLine($"INFO: {GqpHttpTransport.DescribeTlsConfig(options)}");
        var credential = GqpCredentialFactory.CreateInteractiveTokenCredential(options, useDeviceCode: useDeviceCode);

        try
        {
            await GqpCredentialFactory.WarmUpAsync(credential, CancellationToken.None);

            if (!string.IsNullOrWhiteSpace(options.KeyVaultName))
            {
                await GqpSecretsBootstrapper.LoadKeyVaultSecretsAsync(
                    options,
                    credential,
                    logger: null,
                    CancellationToken.None);
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

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Azure.Core.Pipeline;
using System.ClientModel.Primitives;

namespace GqpMcp.Core;

/// <summary>
/// HTTP transport for Azure SDK clients with Baxter corporate TLS support.
/// </summary>
public static class GqpHttpTransport
{
    public static readonly string[] DefaultBaxterCaPaths =
    [
        "/usr/local/share/ca-certificates/proxy-baxter-healthcare.crt",
        "/usr/local/share/ca-certificates/baxter-root-ca-sha2.crt",
    ];

    public static HttpPipelineTransport CreateAzureTransport(GqpOptions options) =>
        new HttpClientTransport(CreateHttpClientHandler(options));

    public static PipelineTransport CreateOpenAiTransport(GqpOptions options)
    {
        var httpClient = new HttpClient(CreateHttpClientHandler(options), disposeHandler: true);
        return new HttpClientPipelineTransport(httpClient);
    }

    public static HttpClientHandler CreateHttpClientHandler(GqpOptions options)
    {
        var handler = new HttpClientHandler();

        if (options.TlsSkipVerify)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            return handler;
        }

        var caFile = ResolveCaFile(options);
        if (string.IsNullOrWhiteSpace(caFile))
        {
            return handler;
        }

        var trustedCa = X509Certificate2.CreateFromPem(File.ReadAllText(caFile));
        handler.ServerCertificateCustomValidationCallback = (_, cert, _, errors) =>
            ValidateServerCertificate(cert, errors, trustedCa);

        return handler;
    }

    public static string? ResolveCaFile(GqpOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.TlsCaFile))
        {
            return options.TlsCaFile;
        }

        foreach (var path in DefaultBaxterCaPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    public static string DescribeTlsConfig(GqpOptions options)
    {
        if (options.TlsSkipVerify)
        {
            return "TLS: certificate validation DISABLED (GQP_TLS_SKIP_VERIFY=true). Dev-only; do not use in production.";
        }

        var caFile = ResolveCaFile(options);
        if (!string.IsNullOrWhiteSpace(caFile))
        {
            var source = string.Equals(caFile, options.TlsCaFile, StringComparison.Ordinal)
                ? "GQP_TLS_CA_FILE"
                : "auto-detected Baxter CA";
            return $"TLS: custom CA trust via {source} ({caFile})";
        }

        return "TLS: system default certificate store (no GQP_TLS_CA_FILE; Baxter CA not auto-detected). "
            + "If you see SSL/certificate errors, set GQP_TLS_CA_FILE or run: sudo update-ca-certificates";
    }

    public static bool IsCertificateError(Exception ex) =>
        ex.Message.Contains("certificate", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("SSL", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase)
        || (ex.InnerException is not null && IsCertificateError(ex.InnerException));

    public static string CertificateErrorHelp(GqpOptions options) =>
        "TLS certificate validation failed. On Baxter WSL/Linux this usually means the corporate proxy CA "
        + "is not trusted by .NET.\n"
        + "Fix (preferred): add to ~/.config/gqp-mcp.env:\n"
        + "  GQP_TLS_CA_FILE=/usr/local/share/ca-certificates/proxy-baxter-healthcare.crt\n"
        + "Or install Baxter CAs system-wide: sudo update-ca-certificates\n"
        + "Dev workaround only: GQP_TLS_SKIP_VERIFY=true\n"
        + $"Current: {DescribeTlsConfig(options)}";

    private static bool ValidateServerCertificate(
        X509Certificate2? cert,
        SslPolicyErrors errors,
        X509Certificate2 trustedCa)
    {
        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        if (cert is null)
        {
            return false;
        }

        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.CustomTrustStore.Add(trustedCa);
        return chain.Build(cert);
    }
}

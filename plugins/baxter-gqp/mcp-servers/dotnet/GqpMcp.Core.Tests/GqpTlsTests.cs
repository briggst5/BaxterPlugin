using GqpMcp.Core;
using Xunit;

namespace GqpMcp.Core.Tests;

public class GqpTlsTests
{
    [Fact]
    public void DescribeTlsConfig_reports_skip_verify()
    {
        var options = new GqpOptions { TlsSkipVerify = true };
        var description = GqpHttpTransport.DescribeTlsConfig(options);
        Assert.Contains("DISABLED", description);
    }

    [Fact]
    public void DescribeTlsConfig_reports_explicit_ca_file()
    {
        var options = new GqpOptions { TlsCaFile = "/tmp/test-ca.pem" };
        var description = GqpHttpTransport.DescribeTlsConfig(options);
        Assert.Contains("GQP_TLS_CA_FILE", description);
        Assert.Contains("/tmp/test-ca.pem", description);
    }

    [Fact]
    public void IsCertificateError_detects_ssl_message()
    {
        Assert.True(GqpHttpTransport.IsCertificateError(new Exception("The SSL connection could not be established")));
    }

    [Fact]
    public void CertificateErrorHelp_mentions_GQP_TLS_CA_FILE()
    {
        var help = GqpHttpTransport.CertificateErrorHelp(new GqpOptions());
        Assert.Contains("GQP_TLS_CA_FILE", help);
        Assert.Contains("GQP_TLS_SKIP_VERIFY", help);
    }
}

using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace PolarionMcp.Client;

/// <summary>
/// Configures TLS for WCF clients on .NET Core via <see cref="HttpClientHandler"/>.
/// <see cref="ServicePointManager.ServerCertificateValidationCallback"/> is ignored
/// for System.ServiceModel.Http on Linux/macOS.
/// </summary>
internal sealed class TlsHttpClientEndpointBehavior : IEndpointBehavior
{
    private readonly Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> _validator;

    private TlsHttpClientEndpointBehavior(
        Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> validator
    )
    {
        _validator = validator;
    }

    public static TlsHttpClientEndpointBehavior? TryCreate(PolarionClientOptions options)
    {
        if (options.TlsSkipVerify)
        {
            return new TlsHttpClientEndpointBehavior((_, _, _, _) => true);
        }

        if (string.IsNullOrWhiteSpace(options.TlsCaFile))
        {
            return null;
        }

        var trustedCa = X509Certificate2.CreateFromPemFile(options.TlsCaFile);
        return new TlsHttpClientEndpointBehavior((_, cert, _, errors) =>
        {
            if (errors == SslPolicyErrors.None)
            {
                return true;
            }

            if (cert is null)
            {
                return false;
            }

            using var customChain = new X509Chain();
            customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            customChain.ChainPolicy.CustomTrustStore.Add(trustedCa);
            return customChain.Build(cert);
        });
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
        bindingParameters.Add(
            new Func<HttpClientHandler, HttpMessageHandler>(handler =>
            {
                handler.ServerCertificateCustomValidationCallback = _validator;
                return handler;
            })
        );
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }
}

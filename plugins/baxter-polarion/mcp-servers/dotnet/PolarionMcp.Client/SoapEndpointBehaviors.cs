using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace PolarionMcp.Client;

internal sealed class PolarionSessionState
{
    public string SessionId { get; set; } = string.Empty;

    public string SessionNamespace { get; set; } = "http://ws.polarion.com/SessionWebService-impl";
}

internal sealed class PolarionSoapEndpointBehavior : IEndpointBehavior
{
    private readonly PolarionSessionState _sessionState;
    private readonly string _authorizationHeader;
    private readonly bool _includeSessionHeader;
    private readonly bool _captureSessionHeader;

    public PolarionSoapEndpointBehavior(
        PolarionSessionState sessionState,
        string authorizationHeader,
        bool includeSessionHeader,
        bool captureSessionHeader
    )
    {
        _sessionState = sessionState;
        _authorizationHeader = authorizationHeader;
        _includeSessionHeader = includeSessionHeader;
        _captureSessionHeader = captureSessionHeader;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(
            new PolarionSoapMessageInspector(
                _sessionState,
                _authorizationHeader,
                _includeSessionHeader,
                _captureSessionHeader
            )
        );
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }
}

internal sealed class PolarionSoapMessageInspector : IClientMessageInspector
{
    private readonly PolarionSessionState _sessionState;
    private readonly string _authorizationHeader;
    private readonly bool _includeSessionHeader;
    private readonly bool _captureSessionHeader;

    public PolarionSoapMessageInspector(
        PolarionSessionState sessionState,
        string authorizationHeader,
        bool includeSessionHeader,
        bool captureSessionHeader
    )
    {
        _sessionState = sessionState;
        _authorizationHeader = authorizationHeader;
        _includeSessionHeader = includeSessionHeader;
        _captureSessionHeader = captureSessionHeader;
    }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        if (!string.IsNullOrWhiteSpace(_authorizationHeader))
        {
            if (!request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out var rawHttpProperty) ||
                rawHttpProperty is not HttpRequestMessageProperty httpProperty)
            {
                httpProperty = new HttpRequestMessageProperty();
                request.Properties[HttpRequestMessageProperty.Name] = httpProperty;
            }

            httpProperty.Headers["Authorization"] = _authorizationHeader;
        }

        if (_includeSessionHeader && !string.IsNullOrWhiteSpace(_sessionState.SessionId))
        {
            request.Headers.Add(
                MessageHeader.CreateHeader(
                    "sessionID",
                    _sessionState.SessionNamespace,
                    _sessionState.SessionId
                )
            );
        }

        return null;
    }

    public void AfterReceiveReply(ref Message reply, object? correlationState)
    {
        if (!_captureSessionHeader)
        {
            return;
        }

        for (var index = 0; index < reply.Headers.Count; index++)
        {
            if (!string.Equals(reply.Headers[index].Name, "sessionID", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var sessionId = reply.Headers.GetHeader<string>(index);
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                continue;
            }

            _sessionState.SessionId = sessionId.Trim();
            _sessionState.SessionNamespace = reply.Headers[index].Namespace;
            return;
        }
    }
}

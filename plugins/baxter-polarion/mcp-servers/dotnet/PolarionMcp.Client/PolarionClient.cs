using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using PolarionMcp.Client.ConnectedServices.SessionWebService;
using PolarionMcp.Client.ConnectedServices.TestManagementWebService;
using PolarionMcp.Client.ConnectedServices.TrackerWebService;
using EnumOptionId = PolarionMcp.Client.ConnectedServices.TrackerWebService.EnumOptionId;
using Text = PolarionMcp.Client.ConnectedServices.TrackerWebService.Text;
using TrackerModule = PolarionMcp.Client.ConnectedServices.TrackerWebService.Module;

namespace PolarionMcp.Client;

public sealed class PolarionClient
{
    private static readonly string[] DefaultFields =
    [
        "id",
        "title",
        "type",
        "status",
        "priority",
        "severity",
        "assignee",
        "description",
        "created",
        "updated",
        "resolution"
    ];

    private const string SessionNamespaceDefault = "http://ws.polarion.com/SessionWebService-impl";

    private static readonly Regex SessionExpiredRegex = new(
        "not authorized|session.*(expired|invalid|timed out)|authentication.*(failed|expired)|401",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    );
    private static readonly object TlsCallbackLock = new();
    private static bool TlsCallbackConfigured;

    private readonly PolarionClientOptions _options;
    private readonly ILogger<PolarionClient> _logger;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly ConcurrentDictionary<string, Func<Dictionary<string, object?>, Dictionary<string, object?>>> _dispatch;
    private readonly PolarionSessionState _sessionState = new()
    {
        SessionNamespace = SessionNamespaceDefault
    };

    private bool _connected;
    private DateTimeOffset? _lastSuccessfulCallUtc;
    private int? _remainingCalls;
    private SessionWebServiceClient? _sessionClient;
    private TrackerWebServiceClient? _trackerClient;
    private TestManagementWebServiceClient? _testManagementClient;

    public PolarionClient(PolarionClientOptions options, ILogger<PolarionClient> logger)
    {
        _options = options;
        _logger = logger;
        _remainingCalls = options.MaxCalls;
        _dispatch = BuildDispatch();
    }

    public void Connect(bool force = false)
    {
        _connectLock.Wait();
        try
        {
            if (_connected && !force)
            {
                return;
            }

            ValidateRequiredConfiguration();
            ConfigureTlsPolicy();
            DisconnectUnlocked();

            var pat = _options.PersonalAccessToken.Trim();
            if (pat.Length > 0)
            {
                try
                {
                    ConnectWithPat(pat);
                }
                catch (Exception patEx)
                {
                    _logger.LogWarning(patEx, "PAT login failed; attempting PAT-as-password fallback.");
                    ConnectWithPasswordLike(_options.User, pat);
                }
            }
            else
            {
                ConnectWithPasswordLike(_options.User, _options.Password);
            }

            _connected = true;
            _lastSuccessfulCallUtc = DateTimeOffset.UtcNow;
            _logger.LogInformation("Polarion connection initialized for user {User}.", _options.User);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public void Disconnect()
    {
        _connectLock.Wait();
        try
        {
            DisconnectUnlocked();
            _logger.LogInformation("Polarion connection state reset.");
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public Dictionary<string, object?> ExecuteTool(string toolName, Dictionary<string, object?> arguments)
    {
        if (!_dispatch.TryGetValue(toolName, out var handler))
        {
            return Error($"Unknown tool '{toolName}'.");
        }

        try
        {
            return ExecuteWithSessionRetry(() => handler(arguments), toolName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for tool {Tool}.", toolName);
            return Error(ex.Message);
        }
    }

    private Dictionary<string, object?> ExecuteWithSessionRetry(Func<Dictionary<string, object?>> operation, string actionName)
    {
        var maxAttempts = Math.Max(1, _options.SessionMaxRetries + 1);
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            MaybeRefreshIdleSession(actionName);
            ConsumeCallBudget(actionName);
            try
            {
                Connect();
                var result = operation();
                _lastSuccessfulCallUtc = DateTimeOffset.UtcNow;
                return result;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsSessionExpiredError(ex))
            {
                _logger.LogInformation(
                    "Polarion session appears stale while executing {Action}; reconnecting (attempt {Attempt}/{MaxAttempts}).",
                    actionName,
                    attempt,
                    maxAttempts
                );
                Disconnect();
                Connect(force: true);
            }
        }

        return Error($"Unable to complete '{actionName}' after session retries.");
    }

    private void MaybeRefreshIdleSession(string actionName)
    {
        if (_options.SessionIdleRefreshSeconds <= 0 || _lastSuccessfulCallUtc is null)
        {
            return;
        }

        var idleFor = DateTimeOffset.UtcNow - _lastSuccessfulCallUtc.Value;
        if (idleFor.TotalSeconds < _options.SessionIdleRefreshSeconds)
        {
            return;
        }

        _logger.LogInformation(
            "Polarion session idle for {IdleSeconds:n0}s before {Action}; reconnecting.",
            idleFor.TotalSeconds,
            actionName
        );

        Disconnect();
        Connect(force: true);
    }

    private void ConsumeCallBudget(string action)
    {
        if (_remainingCalls is null)
        {
            return;
        }

        if (_remainingCalls <= 0)
        {
            throw new InvalidOperationException(
                "Polarion call limit reached. Increase POLARION_MCP_MAX_CALLS or restart the server."
            );
        }

        _remainingCalls -= 1;
        _logger.LogDebug("Polarion call budget: {Remaining} remaining ({Action}).", _remainingCalls, action);
    }

    private void ValidateRequiredConfiguration()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(_options.Url))
        {
            missing.Add("POLARION_URL");
        }

        if (string.IsNullOrWhiteSpace(_options.User))
        {
            missing.Add("POLARION_USER");
        }

        if (string.IsNullOrWhiteSpace(_options.Password) && string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
        {
            missing.Add("POLARION_PASSWORD or POLARION_PAT");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Required environment variable(s) not set: {string.Join(", ", missing)}."
            );
        }
    }

    private static bool IsSessionExpiredError(Exception ex)
    {
        var message = ex.Message ?? string.Empty;
        return SessionExpiredRegex.IsMatch(message);
    }

    private ConcurrentDictionary<string, Func<Dictionary<string, object?>, Dictionary<string, object?>>> BuildDispatch()
    {
        return new ConcurrentDictionary<string, Func<Dictionary<string, object?>, Dictionary<string, object?>>>(StringComparer.Ordinal)
        {
            ["get_work_item"] = GetWorkItem,
            ["set_work_item_status"] = SetWorkItemStatus,
            ["list_work_item_workflow_actions"] = ListWorkItemWorkflowActions,
            ["get_work_item_links"] = GetWorkItemLinks,
            ["add_work_item_link"] = AddWorkItemLink,
            ["remove_work_item_link"] = RemoveWorkItemLink,
            ["get_work_item_raw_fields"] = GetWorkItemRawFields,
            ["query_work_items"] = QueryWorkItems,
            ["list_documents"] = ListDocuments,
            ["list_document_work_items"] = ListDocumentWorkItems,
            ["list_configuration_srs_inventory"] = ListConfigurationSrsInventory,
            ["get_document_text"] = GetDocumentText,
            ["get_document_work_items"] = GetDocumentWorkItems,
            ["list_work_item_comments"] = ListWorkItemComments,
            ["add_work_item_comment"] = AddWorkItemComment,
            ["list_reviewers"] = ListReviewers,
            ["add_reviewer"] = AddReviewer,
            ["remove_reviewer"] = RemoveReviewer,
            ["reset_review_status"] = ResetReviewStatus,
            ["create_test_result_record"] = CreateTestResultRecord
        };
    }

    private void ConnectWithPat(string pat)
    {
        var bearerHeader = $"Bearer {pat}";
        _sessionClient = CreateSessionClient(bearerHeader, captureSessionHeader: true);
        _sessionClient.logInWithTokenAsync("AccessToken", _options.User, pat).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(_sessionState.SessionId))
        {
            throw new InvalidOperationException("Polarion logInWithToken() did not yield a usable session id.");
        }

        _trackerClient = CreateTrackerClient(bearerHeader);
        _testManagementClient = CreateTestManagementClient(bearerHeader);
    }

    private void ConnectWithPasswordLike(string username, string secret)
    {
        var basicHeader = BuildBasicAuthorizationHeader(username, secret);
        _sessionClient = CreateSessionClient(basicHeader, captureSessionHeader: true);
        _sessionClient.logInAsync(username, secret).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(_sessionState.SessionId))
        {
            throw new InvalidOperationException("Polarion logIn() did not yield a usable session id.");
        }

        _trackerClient = CreateTrackerClient(basicHeader);
        _testManagementClient = CreateTestManagementClient(basicHeader);
    }

    private SessionWebServiceClient CreateSessionClient(string authorizationHeader, bool captureSessionHeader)
    {
        var endpoint = new EndpointAddress(BuildWsdlEndpoint("SessionWebService"));
        var client = new SessionWebServiceClient(CreateBinding(), endpoint);
        client.Endpoint.EndpointBehaviors.Add(
            new PolarionSoapEndpointBehavior(
                _sessionState,
                authorizationHeader,
                includeSessionHeader: false,
                captureSessionHeader: captureSessionHeader
            )
        );
        return client;
    }

    private TrackerWebServiceClient CreateTrackerClient(string authorizationHeader)
    {
        var endpoint = new EndpointAddress(BuildWsdlEndpoint("TrackerWebService"));
        var client = new TrackerWebServiceClient(CreateBinding(), endpoint);
        client.Endpoint.EndpointBehaviors.Add(
            new PolarionSoapEndpointBehavior(
                _sessionState,
                authorizationHeader,
                includeSessionHeader: true,
                captureSessionHeader: false
            )
        );
        return client;
    }

    private TestManagementWebServiceClient CreateTestManagementClient(string authorizationHeader)
    {
        var endpoint = new EndpointAddress(BuildWsdlEndpoint("TestManagementWebService"));
        var client = new TestManagementWebServiceClient(CreateBinding(), endpoint);
        client.Endpoint.EndpointBehaviors.Add(
            new PolarionSoapEndpointBehavior(
                _sessionState,
                authorizationHeader,
                includeSessionHeader: true,
                captureSessionHeader: false
            )
        );
        return client;
    }

    private BasicHttpBinding CreateBinding()
    {
        var binding = new BasicHttpBinding
        {
            MaxReceivedMessageSize = 1024L * 1024L * 32L,
            ReaderQuotas =
            {
                MaxStringContentLength = 1024 * 1024 * 4,
                MaxArrayLength = 1024 * 1024 * 4
            },
            AllowCookies = true
        };

        var uri = new Uri(BuildWsdlEndpoint("SessionWebService"));
        binding.Security.Mode = uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            ? BasicHttpSecurityMode.Transport
            : BasicHttpSecurityMode.None;

        return binding;
    }

    private string BuildWsdlEndpoint(string serviceName)
    {
        return $"{_options.Url.TrimEnd('/')}/ws/services/{serviceName}";
    }

    private static string BuildBasicAuthorizationHeader(string username, string password)
    {
        var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
        return $"Basic {Convert.ToBase64String(bytes)}";
    }

    private void ConfigureTlsPolicy()
    {
        lock (TlsCallbackLock)
        {
            if (TlsCallbackConfigured)
            {
                return;
            }

            if (_options.TlsSkipVerify)
            {
                ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
                TlsCallbackConfigured = true;
                _logger.LogWarning("TLS certificate validation is disabled (POLARION_TLS_SKIP_VERIFY=true).");
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.TlsCaFile))
            {
                TlsCallbackConfigured = true;
                return;
            }

            var trustedCa = X509Certificate2.CreateFromPemFile(_options.TlsCaFile);
            ServicePointManager.ServerCertificateValidationCallback += (_, cert, _, errors) =>
            {
                if (errors == System.Net.Security.SslPolicyErrors.None)
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
                return customChain.Build(new X509Certificate2(cert));
            };
            TlsCallbackConfigured = true;
        }
    }

    private void DisconnectUnlocked()
    {
        _connected = false;
        _lastSuccessfulCallUtc = null;
        _sessionState.SessionId = string.Empty;
        _sessionState.SessionNamespace = SessionNamespaceDefault;
        SafeCloseClient(_sessionClient);
        SafeCloseClient(_trackerClient);
        SafeCloseClient(_testManagementClient);
        _sessionClient = null;
        _trackerClient = null;
        _testManagementClient = null;
    }

    private static void SafeCloseClient(ICommunicationObject? client)
    {
        if (client is null)
        {
            return;
        }

        try
        {
            if (client.State == CommunicationState.Faulted)
            {
                client.Abort();
            }
            else
            {
                client.Close();
            }
        }
        catch
        {
            client.Abort();
        }
    }

    private TrackerWebServiceClient TrackerClient()
    {
        return _trackerClient ?? throw new InvalidOperationException("Tracker SOAP client is not initialized.");
    }

    private TestManagementWebServiceClient TestManagementClient()
    {
        return _testManagementClient ?? throw new InvalidOperationException("TestManagement SOAP client is not initialized.");
    }

    private Dictionary<string, object?> GetWorkItem(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var response = TrackerClient().queryWorkItemsLimitedAsync($"id:{workItemId}", "id", DefaultFields, 1).GetAwaiter().GetResult();
        var workItem = GetReturnArray<WorkItem>(response).FirstOrDefault();
        if (workItem is null)
        {
            return Error($"No work item found for '{workItemId}'.");
        }

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["work_item"] = NormalizeObject(workItem)
        };
    }

    private Dictionary<string, object?> SetWorkItemStatus(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var status = GetOptionalString(arguments, "status");
        var workflowActionRaw = GetOptionalString(arguments, "workflow_action_id");
        var uri = BuildWorkItemUri(workItemId);

        var actionsResponse = TrackerClient().getAvailableActionsAsync(uri).GetAwaiter().GetResult();
        var actions = GetReturnArray<WorkflowAction>(actionsResponse);

        int? actionId = null;
        if (!string.IsNullOrWhiteSpace(workflowActionRaw) && int.TryParse(workflowActionRaw, out var parsedActionId))
        {
            actionId = parsedActionId;
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            actionId = actions
                .FirstOrDefault(a =>
                    string.Equals(a.targetStatus?.id, status, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.actionName, status, StringComparison.OrdinalIgnoreCase))?
                .actionId;
        }

        if (actionId is null)
        {
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["error"] = "No matching workflow action found.",
                ["available_actions"] = actions.Select(NormalizeObject).ToArray()
            };
        }

        TrackerClient().performWorkflowActionAsync(uri, actionId.Value).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["success"] = true,
            ["work_item_id"] = workItemId,
            ["workflow_action_id"] = actionId.Value
        };
    }

    private Dictionary<string, object?> ListWorkItemWorkflowActions(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var uri = BuildWorkItemUri(workItemId);
        var response = TrackerClient().getAvailableActionsAsync(uri).GetAwaiter().GetResult();
        var actions = GetReturnArray<WorkflowAction>(response);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["work_item_id"] = workItemId,
            ["actions"] = actions.Select(NormalizeObject).ToArray()
        };
    }

    private Dictionary<string, object?> GetWorkItemLinks(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var uri = BuildWorkItemUri(workItemId);
        var response = TrackerClient().getWorkItemByUriWithFieldsAsync(uri, ["linkedWorkItems", "linkedWorkItemsDerived"]).GetAwaiter().GetResult();
        var wi = GetReturnValue<WorkItem>(response);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["work_item_id"] = workItemId,
            ["links"] = NormalizeObject(wi?.linkedWorkItems) ?? Array.Empty<object>(),
            ["derived_links"] = NormalizeObject(wi?.linkedWorkItemsDerived) ?? Array.Empty<object>()
        };
    }

    private Dictionary<string, object?> AddWorkItemLink(Dictionary<string, object?> arguments)
    {
        var source = GetRequiredString(arguments, "source_work_item_id");
        var target = GetRequiredString(arguments, "target_work_item_id");
        var role = GetOptionalString(arguments, "link_role") ??
                   (Environment.GetEnvironmentVariable("POLARION_DEFAULT_LINK_ROLE") ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(role))
        {
            return Error("link_role is required.");
        }

        TrackerClient().addLinkedItemAsync(BuildWorkItemUri(source), BuildWorkItemUri(target), new EnumOptionId { id = role }).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["success"] = true,
            ["source_work_item_id"] = source,
            ["target_work_item_id"] = target,
            ["link_role"] = role
        };
    }

    private Dictionary<string, object?> RemoveWorkItemLink(Dictionary<string, object?> arguments)
    {
        var source = GetRequiredString(arguments, "source_work_item_id");
        var target = GetRequiredString(arguments, "target_work_item_id");
        var role = GetOptionalString(arguments, "link_role") ??
                   (Environment.GetEnvironmentVariable("POLARION_DEFAULT_LINK_ROLE") ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(role))
        {
            return Error("link_role is required.");
        }

        TrackerClient().removeLinkedItemAsync(BuildWorkItemUri(source), BuildWorkItemUri(target), new EnumOptionId { id = role }).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal) { ["success"] = true };
    }

    private Dictionary<string, object?> GetWorkItemRawFields(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var fields = GetOptionalStringArray(arguments, "fields") ?? DefaultFields;
        var response = TrackerClient().getWorkItemByUriWithFieldsAsync(BuildWorkItemUri(workItemId), fields).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["work_item"] = NormalizeObject(GetReturnValue<WorkItem>(response))
        };
    }

    private Dictionary<string, object?> QueryWorkItems(Dictionary<string, object?> arguments)
    {
        var project = EffectiveProjectId(GetOptionalString(arguments, "project_id"));
        var workItemType = GetOptionalString(arguments, "work_item_type");
        var status = GetOptionalString(arguments, "status");
        var query = GetOptionalString(arguments, "query");
        var limit = GetOptionalInt(arguments, "limit") ?? 50;

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(project))
        {
            filters.Add($"project.id:{project}");
        }

        if (!string.IsNullOrWhiteSpace(workItemType))
        {
            filters.Add($"type:{workItemType}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add($"status:{status}");
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            filters.Add($"({query})");
        }

        var finalQuery = string.Join(" AND ", filters.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (string.IsNullOrWhiteSpace(finalQuery))
        {
            finalQuery = "*:*";
        }

        var response = TrackerClient().queryWorkItemsLimitedAsync(finalQuery, "id", DefaultFields, limit).GetAwaiter().GetResult();
        var items = GetReturnArray<WorkItem>(response).Select(NormalizeObject).ToArray();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["query"] = finalQuery,
            ["count"] = items.Length,
            ["items"] = items
        };
    }

    private Dictionary<string, object?> ListDocuments(Dictionary<string, object?> arguments)
    {
        var project = EffectiveProjectId(GetOptionalString(arguments, "project_id"));
        if (string.IsNullOrWhiteSpace(project))
        {
            return Error("POLARION_PROJECT is required to list documents.");
        }

        var space = GetOptionalString(arguments, "space");
        var limit = GetOptionalInt(arguments, "limit") ?? 200;
        var query = $"project.id:{project}";
        if (!string.IsNullOrWhiteSpace(space))
        {
            query += $" AND moduleFolder:{space}*";
        }

        var fields = new[] { "id", "title", "moduleName", "moduleFolder", "status", "type", "location", "homePageContent" };
        var response = TrackerClient().queryModulesAsync(query, "title", fields, limit).GetAwaiter().GetResult();
        var modules = GetReturnArray<TrackerModule>(response).Select(NormalizeObject).ToArray();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["count"] = modules.Length,
            ["documents"] = modules
        };
    }

    private Dictionary<string, object?> ListDocumentWorkItems(Dictionary<string, object?> arguments)
    {
        var documentName = GetRequiredString(arguments, "document_name");
        var space = GetOptionalString(arguments, "space");
        var limit = GetOptionalInt(arguments, "limit") ?? 200;
        if (limit < 1)
        {
            return Error("limit must be >= 1");
        }

        var projectId = EffectiveProjectId(GetOptionalString(arguments, "project_id"));
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return Error("POLARION_PROJECT is required to list document work items.");
        }

        var (rawName, spaceNorm, clauses) = BuildDocumentWorkItemClauses(documentName, space);
        var docFilter = $"({string.Join(" OR ", clauses)})";
        var query = $"project.id:{projectId} AND {docFilter}";

        try
        {
            var response = TrackerClient().queryWorkItemsLimitedAsync(query, "id", DefaultFields, limit).GetAwaiter().GetResult();
            var workItems = GetReturnArray<WorkItem>(response).Select(NormalizeObject).ToArray();
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["document_name"] = rawName,
                ["space"] = string.IsNullOrWhiteSpace(spaceNorm) ? null : spaceNorm,
                ["project_id"] = projectId,
                ["count"] = workItems.Length,
                ["limit"] = limit,
                ["work_items"] = workItems
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["error"] = ex.Message,
                ["query"] = query,
                ["document_name"] = rawName,
                ["space"] = string.IsNullOrWhiteSpace(spaceNorm) ? null : spaceNorm
            };
        }
    }

    private Dictionary<string, object?> ListConfigurationSrsInventory(Dictionary<string, object?> arguments)
    {
        var result = QueryWorkItems(new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["project_id"] = GetOptionalString(arguments, "project_id"),
            ["work_item_type"] = GetOptionalString(arguments, "work_item_type") ?? "config",
            ["limit"] = GetOptionalInt(arguments, "limit") ?? 512,
            ["query"] = GetOptionalString(arguments, "query")
        });

        result["tool_configured_by"] = GetOptionalString(arguments, "tool_configured_by") ?? "Facility Config";
        return result;
    }

    private Dictionary<string, object?> GetDocumentText(Dictionary<string, object?> arguments)
    {
        var documentName = GetRequiredString(arguments, "document_name");
        var space = GetOptionalString(arguments, "space");
        var includeHome = GetOptionalBool(arguments, "include_home_page_content") ?? true;
        var module = ResolveModule(documentName, space);
        if (module is null)
        {
            return Error($"Document '{documentName}' was not found.");
        }

        var home = includeHome ? module.homePageContent?.content : null;
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["document_name"] = documentName,
            ["space"] = space,
            ["home_page_content"] = home ?? string.Empty,
            ["module"] = NormalizeObject(module)
        };
    }

    private Dictionary<string, object?> GetDocumentWorkItems(Dictionary<string, object?> arguments)
    {
        return ListDocumentWorkItems(arguments);
    }

    private Dictionary<string, object?> ListWorkItemComments(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var response = TrackerClient().getWorkItemByUriWithFieldsAsync(BuildWorkItemUri(workItemId), ["comments"]).GetAwaiter().GetResult();
        var wi = GetReturnValue<WorkItem>(response);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["work_item_id"] = workItemId,
            ["comments"] = NormalizeObject(wi?.comments) ?? Array.Empty<object>()
        };
    }

    private Dictionary<string, object?> AddWorkItemComment(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var text = GetRequiredString(arguments, "text");
        var mime = GetOptionalString(arguments, "mime_type") ?? "text/plain";

        var content = new Text { type = mime, content = text };
        var uri = BuildWorkItemUri(workItemId);

        try
        {
            var resp = TrackerClient().addCommentAsync(uri, string.Empty, content).GetAwaiter().GetResult();
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["success"] = true,
                ["comment_uri"] = resp.addCommentReturn
            };
        }
        catch
        {
            TrackerClient().createCommentAsync(uri, content).GetAwaiter().GetResult();
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["success"] = true
            };
        }
    }

    private Dictionary<string, object?> ListReviewers(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var response = TrackerClient().getWorkItemByUriWithFieldsAsync(BuildWorkItemUri(workItemId), ["approvals"]).GetAwaiter().GetResult();
        var wi = GetReturnValue<WorkItem>(response);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["work_item_id"] = workItemId,
            ["reviewers"] = NormalizeObject(wi?.approvals) ?? Array.Empty<object>()
        };
    }

    private Dictionary<string, object?> AddReviewer(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var reviewerId = GetRequiredString(arguments, "reviewer_id");
        TrackerClient().addApproveeAsync(BuildWorkItemUri(workItemId), reviewerId).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal) { ["success"] = true };
    }

    private Dictionary<string, object?> RemoveReviewer(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var reviewerId = GetRequiredString(arguments, "reviewer_id");
        TrackerClient().removeApproveeAsync(BuildWorkItemUri(workItemId), reviewerId).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal) { ["success"] = true };
    }

    private Dictionary<string, object?> ResetReviewStatus(Dictionary<string, object?> arguments)
    {
        var workItemId = GetRequiredString(arguments, "work_item_id");
        var reviewerId = GetOptionalString(arguments, "reviewer_id");
        var waitingStatus = (Environment.GetEnvironmentVariable("POLARION_APPROVAL_STATUS_WAITING_ID") ?? "waiting").Trim();
        var uri = BuildWorkItemUri(workItemId);

        if (!string.IsNullOrWhiteSpace(reviewerId))
        {
            TrackerClient().editApprovalAsync(uri, reviewerId, new EnumOptionId { id = waitingStatus }).GetAwaiter().GetResult();
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["success"] = true,
                ["reviewer_id"] = reviewerId
            };
        }

        var response = TrackerClient().getWorkItemByUriWithFieldsAsync(uri, ["approvals"]).GetAwaiter().GetResult();
        var wi = GetReturnValue<WorkItem>(response);
        foreach (var approval in wi?.approvals ?? [])
        {
            var id = approval?.user?.id;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            TrackerClient().editApprovalAsync(uri, id, new EnumOptionId { id = waitingStatus }).GetAwaiter().GetResult();
        }

        return new Dictionary<string, object?>(StringComparer.Ordinal) { ["success"] = true };
    }

    private Dictionary<string, object?> CreateTestResultRecord(Dictionary<string, object?> arguments)
    {
        var testRunId = GetRequiredString(arguments, "test_run_id");
        var testCaseId = GetRequiredString(arguments, "test_case_id");
        var passed = GetOptionalBool(arguments, "passed") ?? false;
        var comment = GetOptionalString(arguments, "comment") ?? string.Empty;
        var projectId = EffectiveProjectId(GetOptionalString(arguments, "project_id"));
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return Error("POLARION_PROJECT (or project_id) is required for test result creation.");
        }

        var passedEnum = (Environment.GetEnvironmentVariable("POLARION_TEST_RESULT_PASSED_ID") ?? "passed").Trim();
        var failedEnum = (Environment.GetEnvironmentVariable("POLARION_TEST_RESULT_FAILED_ID") ?? "failed").Trim();
        var resultEnum = passed ? passedEnum : failedEnum;

        var testRunUri = BuildWorkItemUri(testRunId, projectId);
        var testCaseUri = BuildWorkItemUri(testCaseId, projectId);

        var record = new TestRecord
        {
            testCaseURI = testCaseUri,
            result = new ConnectedServices.TestManagementWebService.EnumOptionId { id = resultEnum },
            comment = new ConnectedServices.TestManagementWebService.Text { type = "text/plain", content = comment },
            executed = DateTime.UtcNow,
            executedSpecified = true
        };

        TestManagementClient().addTestRecordToTestRunAsync(testRunUri, record).GetAwaiter().GetResult();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["success"] = true,
            ["test_run_id"] = testRunId,
            ["test_case_id"] = testCaseId,
            ["result"] = resultEnum
        };
    }

    private TrackerModule? ResolveModule(string documentName, string? space)
    {
        var project = EffectiveProjectId(null);
        if (string.IsNullOrWhiteSpace(project))
        {
            return null;
        }

        var query = $"project.id:{project}";
        if (!string.IsNullOrWhiteSpace(space))
        {
            query += $" AND moduleFolder:{space}*";
        }

        var fields = new[] { "id", "title", "moduleName", "moduleFolder", "location", "homePageContent" };
        var response = TrackerClient().queryModulesAsync(query, "title", fields, 512).GetAwaiter().GetResult();
        var modules = GetReturnArray<TrackerModule>(response);
        return modules.FirstOrDefault(m =>
            string.Equals(m.title, documentName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(m.moduleName, documentName, StringComparison.OrdinalIgnoreCase));
    }

    private static (string DocumentName, string Space, string[] Clauses) BuildDocumentWorkItemClauses(string documentName, string? space)
    {
        var rawName = (documentName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(rawName))
        {
            throw new ArgumentException("document_name is required", nameof(documentName));
        }

        var decodedName = (WebUtility.UrlDecode(rawName) ?? string.Empty).Trim();
        var spaceNorm = (space ?? string.Empty).Trim().Trim('/');
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(spaceNorm))
        {
            candidates.Add($"document.moduleFolder:\"{EscapeLucenePhrase($"{spaceNorm}/{rawName}")}\"");
            if (!string.IsNullOrWhiteSpace(decodedName) &&
                !string.Equals(decodedName, rawName, StringComparison.Ordinal))
            {
                candidates.Add($"document.moduleFolder:\"{EscapeLucenePhrase($"{spaceNorm}/{decodedName}")}\"");
            }
        }

        candidates.Add($"document.title:\"{EscapeLucenePhrase(rawName)}\"");
        if (!string.IsNullOrWhiteSpace(decodedName) &&
            !string.Equals(decodedName, rawName, StringComparison.Ordinal))
        {
            candidates.Add($"document.title:\"{EscapeLucenePhrase(decodedName)}\"");
        }

        var uniqueClauses = candidates.Distinct(StringComparer.Ordinal).ToArray();
        return (rawName, spaceNorm, uniqueClauses);
    }

    private static string EscapeLucenePhrase(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private string BuildWorkItemUri(string workItemId)
    {
        var configuredProject = EffectiveProjectId(null);
        var projectId = !string.IsNullOrWhiteSpace(configuredProject)
            ? configuredProject
            : EffectiveProjectId(ExtractProjectPrefix(workItemId));
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidOperationException(
                "Cannot build work item URI: set POLARION_PROJECT or use PROJECT-ID formatted work item ids."
            );
        }

        return BuildWorkItemUri(workItemId, projectId);
    }

    private static string BuildWorkItemUri(string workItemId, string projectId)
    {
        return $"subterra:data-service:objects:/default/{projectId}${{WorkItem}}{workItemId}";
    }

    private string EffectiveProjectId(string? candidate)
    {
        if (!string.IsNullOrWhiteSpace(candidate))
        {
            return candidate.Trim();
        }

        return _options.Project.Trim();
    }

    private static string? ExtractProjectPrefix(string workItemId)
    {
        var idx = workItemId.IndexOf('-', StringComparison.Ordinal);
        return idx > 0 ? workItemId[..idx] : null;
    }

    private static object? NormalizeObject(object? obj)
    {
        return NormalizeObject(obj, new HashSet<object>(ReferenceEqualityComparer.Instance), 0);
    }

    private static object? NormalizeObject(object? obj, HashSet<object> seen, int depth)
    {
        if (obj is null)
        {
            return null;
        }

        if (depth > 12)
        {
            return "...";
        }

        if (obj is string || obj is bool || obj is int || obj is long || obj is short ||
            obj is float || obj is double || obj is decimal)
        {
            return obj;
        }

        if (obj is DateTime dt)
        {
            return dt.ToUniversalTime().ToString("O");
        }

        if (obj is DateTimeOffset dto)
        {
            return dto.ToUniversalTime().ToString("O");
        }

        if (obj is IEnumerable enumerable && obj is not string)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
            {
                list.Add(NormalizeObject(item, seen, depth + 1));
            }

            return list;
        }

        var type = obj.GetType();
        if (!type.IsClass)
        {
            return obj;
        }

        if (!seen.Add(obj))
        {
            return "<cycle>";
        }

        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in type.GetProperties())
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (property.Name.EndsWith("Specified", StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                var value = property.GetValue(obj);
                dict[property.Name] = NormalizeObject(value, seen, depth + 1);
            }
            catch
            {
                // ignore generated proxy reflection exceptions
            }
        }

        return dict;
    }

    private static T[] GetReturnArray<T>(object response)
    {
        var returnValue = FindReturnMemberValue(response);
        return returnValue as T[] ?? [];
    }

    private static T? GetReturnValue<T>(object response) where T : class
    {
        return FindReturnMemberValue(response) as T;
    }

    private static object? FindReturnMemberValue(object response)
    {
        var type = response.GetType();
        var property = type.GetProperties().FirstOrDefault(p => p.Name.EndsWith("Return", StringComparison.Ordinal));
        if (property is not null)
        {
            return property.GetValue(response);
        }

        var field = type.GetFields().FirstOrDefault(f => f.Name.EndsWith("Return", StringComparison.Ordinal));
        return field?.GetValue(response);
    }

    private static string GetRequiredString(Dictionary<string, object?> args, string key)
    {
        var value = GetOptionalString(args, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{key} is required.");
        }

        return value.Trim();
    }

    private static string? GetOptionalString(Dictionary<string, object?> args, string key)
    {
        return args.TryGetValue(key, out var raw) && raw is not null ? raw.ToString() : null;
    }

    private static int? GetOptionalInt(Dictionary<string, object?> args, string key)
    {
        var raw = GetOptionalString(args, key);
        return int.TryParse(raw, out var parsed) ? parsed : null;
    }

    private static bool? GetOptionalBool(Dictionary<string, object?> args, string key)
    {
        if (!args.TryGetValue(key, out var raw) || raw is null)
        {
            return null;
        }

        if (raw is bool boolValue)
        {
            return boolValue;
        }

        return raw.ToString()?.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "yes" => true,
            "0" or "false" or "no" => false,
            _ => null
        };
    }

    private static string[]? GetOptionalStringArray(Dictionary<string, object?> args, string key)
    {
        if (!args.TryGetValue(key, out var raw) || raw is null)
        {
            return null;
        }

        if (raw is string[] values)
        {
            return values;
        }

        if (raw is IEnumerable enumerable && raw is not string)
        {
            return enumerable.Cast<object?>().Select(x => x?.ToString() ?? string.Empty).ToArray();
        }

        return null;
    }

    private static Dictionary<string, object?> Error(string message)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["error"] = message
        };
    }
}

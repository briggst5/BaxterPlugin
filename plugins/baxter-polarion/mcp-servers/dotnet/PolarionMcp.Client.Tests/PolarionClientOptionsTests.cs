using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using PolarionMcp.Client;
using PolarionMcp.Client.ConnectedServices.TrackerWebService;

namespace PolarionMcp.Client.Tests;

public sealed class PolarionClientOptionsTests
{
    [Fact]
    public void FromEnvironment_ParsesSessionAndTlsFlags()
    {
        using var env = new EnvironmentScope(new Dictionary<string, string?>
        {
            ["POLARION_URL"] = "https://polarion.local/polarion",
            ["POLARION_USER"] = "user1",
            ["POLARION_PASSWORD"] = "secret",
            ["POLARION_TLS_SKIP_VERIFY"] = "false",
            ["POLARION_SESSION_MAX_RETRIES"] = "2",
            ["POLARION_SESSION_IDLE_REFRESH_SECONDS"] = "300",
            ["POLARION_MCP_MAX_CALLS"] = "9"
        });

        var options = PolarionClientOptions.FromEnvironment();

        Assert.False(options.TlsSkipVerify);
        Assert.Equal(2, options.SessionMaxRetries);
        Assert.Equal(300, options.SessionIdleRefreshSeconds);
        Assert.Equal(9, options.MaxCalls);
    }

    [Fact]
    public void ExecuteTool_ReturnsError_WhenToolUnknown()
    {
        var options = new PolarionClientOptions
        {
            Url = "https://polarion.local/polarion",
            User = "user1",
            Password = "secret"
        };

        var client = new PolarionClient(options, NullLogger<PolarionClient>.Instance);
        var result = client.ExecuteTool("unknown_tool", new Dictionary<string, object?>());

        Assert.True(result.ContainsKey("error"));
    }

    [Fact]
    public void ExecuteTool_ReturnsErrorPayload_ForKnownTool()
    {
        var options = new PolarionClientOptions
        {
            Url = "https://polarion.local/polarion",
            User = "user1",
            Password = "secret"
        };

        var client = new PolarionClient(options, NullLogger<PolarionClient>.Instance);
        var result = client.ExecuteTool(
            "get_work_item",
            new Dictionary<string, object?> { ["work_item_id"] = "PLT1-100" }
        );

        Assert.True(result.TryGetValue("error", out var message));
        Assert.False(string.IsNullOrWhiteSpace(message?.ToString()));
    }

    [Fact]
    public void TryResolveCommentParentUri_ReturnsSubterraUriAsIs()
    {
        var parentUri = "subterra:data-service:objects:/default/PLT${WorkItem}PLT-1${Comment}1";

        var result = InvokeTryResolveCommentParentUri(parentUri, comments: null);

        Assert.Equal(parentUri, result);
    }

    [Fact]
    public void TryResolveCommentParentUri_ResolvesListedCommentId()
    {
        var comments = new[]
        {
            new Comment
            {
                id = "1",
                uri = "subterra:data-service:objects:/default/PLT${WorkItem}PLT-1${Comment}1"
            },
            new Comment
            {
                id = "2",
                uri = "subterra:data-service:objects:/default/PLT${WorkItem}PLT-1${Comment}2"
            }
        };

        var result = InvokeTryResolveCommentParentUri("2", comments);

        Assert.Equal("subterra:data-service:objects:/default/PLT${WorkItem}PLT-1${Comment}2", result);
    }

    [Fact]
    public void TryResolveCommentParentUri_ReturnsNull_WhenCommentIdUnknown()
    {
        var comments = new[]
        {
            new Comment
            {
                id = "1",
                uri = "subterra:data-service:objects:/default/PLT${WorkItem}PLT-1${Comment}1"
            }
        };

        var result = InvokeTryResolveCommentParentUri("missing", comments);

        Assert.Null(result);
    }

    private static string? InvokeTryResolveCommentParentUri(string parentCommentId, Comment[]? comments)
    {
        var method = typeof(PolarionClient).GetMethod("TryResolveCommentParentUri", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        return (string?)method.Invoke(null, [parentCommentId, comments]);
    }

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues = new(StringComparer.Ordinal);

        public EnvironmentScope(IDictionary<string, string?> updates)
        {
            foreach (var kv in updates)
            {
                _originalValues[kv.Key] = Environment.GetEnvironmentVariable(kv.Key);
                Environment.SetEnvironmentVariable(kv.Key, kv.Value);
            }
        }

        public void Dispose()
        {
            foreach (var kv in _originalValues)
            {
                Environment.SetEnvironmentVariable(kv.Key, kv.Value);
            }
        }
    }
}

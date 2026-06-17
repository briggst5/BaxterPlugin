using Microsoft.Extensions.Logging.Abstractions;
using PolarionMcp.Client;

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

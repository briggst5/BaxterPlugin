using GqpMcp.Core;
using Xunit;

namespace GqpMcp.Core.Tests;

public class GqpEnvFileLoaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalHome;

    public GqpEnvFileLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _originalHome = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty;
        Environment.SetEnvironmentVariable("HOME", _tempDir);
        Environment.SetEnvironmentVariable("USERPROFILE", _tempDir);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("HOME", _originalHome);
        Environment.SetEnvironmentVariable("USERPROFILE", _originalHome);
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void LoadEnvironmentVariables_reads_config_file()
    {
        var configDir = Path.Combine(_tempDir, ".config");
        Directory.CreateDirectory(configDir);
        var configPath = Path.Combine(configDir, "gqp-mcp.env");
        File.WriteAllText(configPath, "GQP_KEYVAULT_NAME=kv-test\n# comment\nAZURE_SEARCH_ENDPOINT=https://example.search.windows.net\n");

        GqpEnvFileLoader.LoadEnvironmentVariables();

        Assert.Equal("kv-test", Environment.GetEnvironmentVariable("GQP_KEYVAULT_NAME"));
        Assert.Equal("https://example.search.windows.net", Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT"));
    }
}

public class GqpKnowledgeServiceTests
{
    [Fact]
    public void FormatHits_returns_no_results_message_when_empty()
    {
        var formatted = GqpKnowledgeService.FormatHits(Array.Empty<SearchHit>());
        Assert.Equal("No results found.", formatted);
    }

    [Fact]
    public void FormatHits_includes_doc_id_and_content()
    {
        var formatted = GqpKnowledgeService.FormatHits(
        [
            new SearchHit
            {
                DocId = "GQP-09-05",
                Revision = "D",
                Title = "Software Lifecycle",
                SectionHeading = "VERIFICATION",
                Content = "Verification shall be documented.",
                PageNumber = "12",
            },
        ]);

        Assert.Contains("GQP-09-05", formatted);
        Assert.Contains("Verification shall be documented.", formatted);
    }
}

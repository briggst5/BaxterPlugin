using Microsoft.Extensions.Logging;
using PolarionMcp.Server;

namespace PolarionMcp.Client.Tests;

public sealed class ServerLoggingTests
{
    [Theory]
    [InlineData(null, LogLevel.Warning)]
    [InlineData("", LogLevel.Warning)]
    [InlineData("WARNING", LogLevel.Warning)]
    [InlineData("debug", LogLevel.Debug)]
    [InlineData("INFO", LogLevel.Information)]
    [InlineData("warn", LogLevel.Warning)]
    [InlineData("ERROR", LogLevel.Error)]
    [InlineData("fatal", LogLevel.Critical)]
    [InlineData("off", LogLevel.None)]
    public void ParseLogLevel_ReturnsConfiguredLogLevel(string? rawLogLevel, LogLevel expected)
    {
        var logLevel = ServerLogging.ParseLogLevel(rawLogLevel);

        Assert.Equal(expected, logLevel);
    }

    [Fact]
    public void ParseLogLevel_ThrowsForUnknownLogLevel()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => ServerLogging.ParseLogLevel("verbose"));

        Assert.Contains("POLARION_MCP_LOG_LEVEL", exception.Message);
    }
}

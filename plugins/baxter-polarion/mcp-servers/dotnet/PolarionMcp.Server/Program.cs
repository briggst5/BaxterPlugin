using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using PolarionMcp.Client;
using PolarionMcp.Server;

PolarionEnvFileLoader.LoadEnvironmentVariables();

var options = PolarionClientOptions.FromEnvironment();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(consoleOptions =>
{
    // stdout must remain protocol-only for MCP stdio servers.
    consoleOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

if (options.TlsSkipVerify)
{
    Console.Error.WriteLine("WARN: POLARION_TLS_SKIP_VERIFY=true; TLS certificate validation is disabled.");
}

builder.Services.AddSingleton(options);
builder.Services.AddSingleton<PolarionClient>();
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

using GqpMcp.Core;
using GqpMcp.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

if (args.Length > 0)
{
    if (string.Equals(args[0], "authenticate", StringComparison.OrdinalIgnoreCase))
    {
        Environment.Exit(await GqpStartup.RunAuthenticateAsync(args));
    }

    if (string.Equals(args[0], "check-auth", StringComparison.OrdinalIgnoreCase))
    {
        if (!GqpEnvFileLoader.ConfigFileExists())
        {
            Console.Error.WriteLine($"GQP MCP is not configured. Create {GqpEnvFileLoader.ConfigPath}.");
            Environment.Exit(1);
        }

        Environment.Exit(await GqpStartup.RunCheckAuthAsync());
    }
}

if (!GqpEnvFileLoader.ConfigFileExists())
{
    Console.Error.WriteLine("GQP MCP is not configured.");
    Console.Error.WriteLine($"Create {GqpEnvFileLoader.ConfigPath} (see gqp-mcp.env.example).");
    Console.Error.WriteLine("Quick setup: node scripts/setup-gqp-env.mjs");
    Console.Error.WriteLine("Or enable gqp-knowledge in Cursor MCP — the launcher creates config automatically.");
    Environment.Exit(1);
}

var bootstrapOptions = GqpStartup.LoadOptions();
if (bootstrapOptions.TlsSkipVerify)
{
    Console.Error.WriteLine("WARN: GQP_TLS_SKIP_VERIFY=true; TLS certificate validation is disabled.");
}
else
{
    Console.Error.WriteLine($"INFO: {GqpHttpTransport.DescribeTlsConfig(bootstrapOptions)}");
}

var logLevel = ParseLogLevel(bootstrapOptions.LogLevel);

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(logLevel);
builder.Logging.AddConsole(consoleOptions =>
{
    consoleOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(bootstrapOptions);
builder.Services.AddSingleton<GqpKnowledgeService>();
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

static LogLevel ParseLogLevel(string value) =>
    Enum.TryParse<LogLevel>(value, ignoreCase: true, out var level) ? level : LogLevel.Information;

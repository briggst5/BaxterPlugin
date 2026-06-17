using System.Text.Json;
using Microsoft.Extensions.Logging;
using PolarionMcp.Client;

LoadEnvFile();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<PolarionClient>();
var client = new PolarionClient(PolarionClientOptions.FromEnvironment(), logger);

Console.WriteLine("== Polarion .NET smoke test ==");

var results = new List<(string Tool, bool Passed, string? Error)>();
var mutableResults = new List<(string Tool, bool Passed, string? Error)>();

var query = Execute("query_work_items", new Dictionary<string, object?> { ["limit"] = 1 }, results);
var docs = Execute("list_documents", new Dictionary<string, object?> { ["limit"] = 1 }, results);

var maybeId = TryExtractFirstWorkItemId(query.Payload);
var (documentName, documentSpace) = TryExtractDocumentNameAndSpace(docs.Payload);

if (!string.IsNullOrWhiteSpace(maybeId))
{
    Execute("get_work_item", new Dictionary<string, object?> { ["work_item_id"] = maybeId }, results);
    Execute("list_work_item_workflow_actions", new Dictionary<string, object?> { ["work_item_id"] = maybeId }, results);
    Execute("get_work_item_links", new Dictionary<string, object?> { ["work_item_id"] = maybeId }, results);
    Execute("get_work_item_raw_fields", new Dictionary<string, object?> { ["work_item_id"] = maybeId }, results);
    Execute("list_work_item_comments", new Dictionary<string, object?> { ["work_item_id"] = maybeId }, results);
    Execute("list_reviewers", new Dictionary<string, object?> { ["work_item_id"] = maybeId }, results);
}
else
{
    Console.WriteLine("Skipping work-item dependent calls: no work item id found.");
}

if (!string.IsNullOrWhiteSpace(documentName))
{
    Execute(
        "list_document_work_items",
        new Dictionary<string, object?>
        {
            ["document_name"] = documentName,
            ["space"] = documentSpace,
            ["limit"] = 5
        },
        results);
    Execute(
        "get_document_text",
        new Dictionary<string, object?>
        {
            ["document_name"] = documentName,
            ["space"] = documentSpace
        },
        results);
    Execute(
        "get_document_work_items",
        new Dictionary<string, object?>
        {
            ["document_name"] = documentName,
            ["space"] = documentSpace,
            ["limit"] = 5
        },
        results);
}
else
{
    Console.WriteLine("Skipping document-dependent calls: no document found.");
}

Execute("list_configuration_srs_inventory", new Dictionary<string, object?> { ["limit"] = 10 }, results);

Console.WriteLine("\n== Read-only tool summary ==");
foreach (var result in results)
{
    Console.WriteLine(
        result.Passed
            ? $"PASS  {result.Tool}"
            : $"FAIL  {result.Tool}: {result.Error}");
}

var mutableTarget = Environment.GetEnvironmentVariable("POLARION_MUTABLE_WORK_ITEM_ID") ?? "PLT1-2668";
Console.WriteLine($"\n== Mutable smoke target: {mutableTarget} ==");

var beforeReviewers = Execute(
    "list_reviewers",
    new Dictionary<string, object?> { ["work_item_id"] = mutableTarget },
    mutableResults);

var commentText = $"[dotnet mutable smoke] {DateTimeOffset.UtcNow:O}";
Execute(
    "add_work_item_comment",
    new Dictionary<string, object?>
    {
        ["work_item_id"] = mutableTarget,
        ["text"] = commentText,
        ["mime_type"] = "text/plain"
    },
    mutableResults);

var peerId = maybeId;
if (string.IsNullOrWhiteSpace(peerId) || string.Equals(peerId, mutableTarget, StringComparison.OrdinalIgnoreCase))
{
    var candidateQuery = Execute(
        "query_work_items",
        new Dictionary<string, object?> { ["limit"] = 3 },
        mutableResults);
    peerId = TryExtractDifferentWorkItemId(candidateQuery.Payload, mutableTarget);
}

if (!string.IsNullOrWhiteSpace(peerId))
{
    var addLink = Execute(
        "add_work_item_link",
        new Dictionary<string, object?>
        {
            ["source_work_item_id"] = mutableTarget,
            ["target_work_item_id"] = peerId,
            ["link_role"] = "relates_to"
        },
        mutableResults);

    if (addLink.Passed)
    {
        Execute(
            "remove_work_item_link",
            new Dictionary<string, object?>
            {
                ["source_work_item_id"] = mutableTarget,
                ["target_work_item_id"] = peerId,
                ["link_role"] = "relates_to"
            },
            mutableResults);
    }
}
else
{
    mutableResults.Add(("add_work_item_link/remove_work_item_link", false, "No peer work item available for reversible link test."));
}

var currentUser = Environment.GetEnvironmentVariable("POLARION_USER");
var existingReviewerSet = ExtractReviewerIds(beforeReviewers.Payload);
if (!string.IsNullOrWhiteSpace(currentUser))
{
    var addReviewer = Execute(
        "add_reviewer",
        new Dictionary<string, object?>
        {
            ["work_item_id"] = mutableTarget,
            ["reviewer_id"] = currentUser
        },
        mutableResults);

    if (addReviewer.Passed && !existingReviewerSet.Contains(currentUser))
    {
        Execute(
            "remove_reviewer",
            new Dictionary<string, object?>
            {
                ["work_item_id"] = mutableTarget,
                ["reviewer_id"] = currentUser
            },
            mutableResults);
    }
    else if (existingReviewerSet.Contains(currentUser))
    {
        mutableResults.Add(("remove_reviewer", true, "Skipped removal because reviewer existed before test."));
    }
}
else
{
    mutableResults.Add(("add_reviewer/remove_reviewer", false, "POLARION_USER is not set."));
}

Execute(
    "reset_review_status",
    new Dictionary<string, object?>
    {
        ["work_item_id"] = mutableTarget
    },
    mutableResults);

var actions = Execute(
    "list_work_item_workflow_actions",
    new Dictionary<string, object?> { ["work_item_id"] = mutableTarget },
    mutableResults);
var actionId = TryExtractFirstActionId(actions.Payload);
if (!string.IsNullOrWhiteSpace(actionId))
{
    Execute(
        "set_work_item_status",
        new Dictionary<string, object?>
        {
            ["work_item_id"] = mutableTarget,
            ["workflow_action_id"] = actionId
        },
        mutableResults);
}
else
{
    mutableResults.Add(("set_work_item_status", false, "No workflow action available to test."));
}

var testRunId = Environment.GetEnvironmentVariable("POLARION_TEST_RUN_ID");
if (!string.IsNullOrWhiteSpace(testRunId))
{
    Execute(
        "create_test_result_record",
        new Dictionary<string, object?>
        {
            ["test_run_id"] = testRunId,
            ["test_case_id"] = mutableTarget,
            ["passed"] = true,
            ["comment"] = "[dotnet mutable smoke]"
        },
        mutableResults);
}
else
{
    mutableResults.Add(("create_test_result_record", false, "Skipped: set POLARION_TEST_RUN_ID to enable this test."));
}

Console.WriteLine("\n== Mutable tool summary ==");
foreach (var result in mutableResults)
{
    Console.WriteLine(
        result.Passed
            ? $"PASS  {result.Tool}{(string.IsNullOrWhiteSpace(result.Error) ? string.Empty : $" ({result.Error})")}"
            : $"FAIL  {result.Tool}: {result.Error}");
}

void Print(string title, Dictionary<string, object?> payload)
{
    Console.WriteLine($"\n--- {title} ---");
    Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
}

(Dictionary<string, object?> Payload, bool Passed, string? Error) Execute(
    string toolName,
    Dictionary<string, object?> args,
    List<(string Tool, bool Passed, string? Error)> sink)
{
    var payload = client.ExecuteTool(toolName, args);
    Print(toolName, payload);
    var hasError = payload.TryGetValue("error", out var errObj) && !string.IsNullOrWhiteSpace(errObj?.ToString());
    var error = hasError ? errObj?.ToString() : null;
    sink.Add((toolName, !hasError, error));
    return (payload, !hasError, error);
}

static string? TryExtractFirstWorkItemId(Dictionary<string, object?> response)
{
    if (!response.TryGetValue("items", out var itemsObj) || itemsObj is null)
    {
        return null;
    }

    var json = JsonSerializer.Serialize(itemsObj);
    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
    {
        return null;
    }

    var first = doc.RootElement[0];
    if (first.ValueKind == JsonValueKind.Object &&
        first.TryGetProperty("id", out var idElement) &&
        idElement.ValueKind == JsonValueKind.String)
    {
        return idElement.GetString();
    }

    return null;
}

static (string? Name, string? Space) TryExtractDocumentNameAndSpace(Dictionary<string, object?> response)
{
    if (!response.TryGetValue("documents", out var docsObj) || docsObj is null)
    {
        return (null, null);
    }

    var json = JsonSerializer.Serialize(docsObj);
    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
    {
        return (null, null);
    }

    var first = doc.RootElement[0];
    if (first.ValueKind != JsonValueKind.Object)
    {
        return (null, null);
    }

    string? name = null;
    if (first.TryGetProperty("moduleName", out var moduleName) && moduleName.ValueKind == JsonValueKind.String)
    {
        name = moduleName.GetString();
    }
    if (string.IsNullOrWhiteSpace(name) &&
        first.TryGetProperty("title", out var title) &&
        title.ValueKind == JsonValueKind.String)
    {
        name = title.GetString();
    }

    string? space = null;
    if (first.TryGetProperty("moduleFolder", out var moduleFolder) && moduleFolder.ValueKind == JsonValueKind.String)
    {
        var folder = moduleFolder.GetString();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            var parts = folder.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
            {
                space = string.Join('/', parts[..^1]);
            }
        }
    }

    return (name, space);
}

static string? TryExtractDifferentWorkItemId(Dictionary<string, object?> response, string excluded)
{
    if (!response.TryGetValue("items", out var itemsObj) || itemsObj is null)
    {
        return null;
    }

    var json = JsonSerializer.Serialize(itemsObj);
    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.ValueKind != JsonValueKind.Array)
    {
        return null;
    }

    foreach (var item in doc.RootElement.EnumerateArray())
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            continue;
        }

        if (!item.TryGetProperty("id", out var idElement) || idElement.ValueKind != JsonValueKind.String)
        {
            continue;
        }

        var id = idElement.GetString();
        if (!string.IsNullOrWhiteSpace(id) && !string.Equals(id, excluded, StringComparison.OrdinalIgnoreCase))
        {
            return id;
        }
    }

    return null;
}

static HashSet<string> ExtractReviewerIds(Dictionary<string, object?> response)
{
    var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (!response.TryGetValue("reviewers", out var reviewersObj) || reviewersObj is null)
    {
        return ids;
    }

    var json = JsonSerializer.Serialize(reviewersObj);
    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.ValueKind != JsonValueKind.Array)
    {
        return ids;
    }

    foreach (var reviewer in doc.RootElement.EnumerateArray())
    {
        if (reviewer.ValueKind != JsonValueKind.Object)
        {
            continue;
        }

        if (reviewer.TryGetProperty("user", out var userObj) &&
            userObj.ValueKind == JsonValueKind.Object &&
            userObj.TryGetProperty("id", out var idElement) &&
            idElement.ValueKind == JsonValueKind.String)
        {
            var id = idElement.GetString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                ids.Add(id);
            }
        }
    }

    return ids;
}

static string? TryExtractFirstActionId(Dictionary<string, object?> response)
{
    if (!response.TryGetValue("actions", out var actionsObj) || actionsObj is null)
    {
        return null;
    }

    var json = JsonSerializer.Serialize(actionsObj);
    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
    {
        return null;
    }

    var first = doc.RootElement[0];
    if (first.ValueKind != JsonValueKind.Object)
    {
        return null;
    }

    if (first.TryGetProperty("actionId", out var actionId) && actionId.ValueKind == JsonValueKind.Number)
    {
        return actionId.GetInt32().ToString();
    }

    return null;
}

static void LoadEnvFile()
{
    var configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config",
        "polarion-mcp.env");

    if (!File.Exists(configPath))
    {
        return;
    }

    foreach (var rawLine in File.ReadLines(configPath))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            continue;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();
        if (key.Length > 0)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

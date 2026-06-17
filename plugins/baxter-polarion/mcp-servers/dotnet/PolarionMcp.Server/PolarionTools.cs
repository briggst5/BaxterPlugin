using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using PolarionMcp.Client;

namespace PolarionMcp.Server;

[McpServerToolType]
public sealed class PolarionTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly PolarionClient _client;

    public PolarionTools(PolarionClient client)
    {
        _client = client;
    }

    [McpServerTool(Name = "get_work_item"), Description("Retrieve a single Polarion work item by ID.")]
    public string GetWorkItem(string work_item_id) => Run("get_work_item", new() { ["work_item_id"] = work_item_id });

    [McpServerTool(Name = "set_work_item_status"), Description("Change a work item's status/state using Polarion workflow.")]
    public string SetWorkItemStatus(string work_item_id, string? status = null, string? workflow_action_id = null) =>
        Run(
            "set_work_item_status",
            new()
            {
                ["work_item_id"] = work_item_id,
                ["status"] = status,
                ["workflow_action_id"] = workflow_action_id
            }
        );

    [McpServerTool(Name = "list_work_item_workflow_actions"), Description("List valid workflow actions for a work item.")]
    public string ListWorkItemWorkflowActions(string work_item_id) =>
        Run("list_work_item_workflow_actions", new() { ["work_item_id"] = work_item_id });

    [McpServerTool(Name = "get_work_item_links"), Description("Return linked work items for a work item.")]
    public string GetWorkItemLinks(string work_item_id) => Run("get_work_item_links", new() { ["work_item_id"] = work_item_id });

    [McpServerTool(Name = "add_work_item_link"), Description("Add a link between two work items.")]
    public string AddWorkItemLink(string source_work_item_id, string target_work_item_id, string? link_role = null) =>
        Run(
            "add_work_item_link",
            new()
            {
                ["source_work_item_id"] = source_work_item_id,
                ["target_work_item_id"] = target_work_item_id,
                ["link_role"] = link_role
            }
        );

    [McpServerTool(Name = "remove_work_item_link"), Description("Remove a link between two work items.")]
    public string RemoveWorkItemLink(string source_work_item_id, string target_work_item_id, string? link_role = null) =>
        Run(
            "remove_work_item_link",
            new()
            {
                ["source_work_item_id"] = source_work_item_id,
                ["target_work_item_id"] = target_work_item_id,
                ["link_role"] = link_role
            }
        );

    [McpServerTool(Name = "get_work_item_raw_fields"), Description("Return raw SOAP fields for diagnostics.")]
    public string GetWorkItemRawFields(string work_item_id, string[]? fields = null) =>
        Run("get_work_item_raw_fields", new() { ["work_item_id"] = work_item_id, ["fields"] = fields });

    [McpServerTool(Name = "query_work_items"), Description("Query work items by filters and optional Lucene query.")]
    public string QueryWorkItems(string? work_item_type = null, string? status = null, string? query = null, int limit = 50) =>
        Run(
            "query_work_items",
            new()
            {
                ["work_item_type"] = work_item_type,
                ["status"] = status,
                ["query"] = query,
                ["limit"] = limit
            }
        );

    [McpServerTool(Name = "list_documents"), Description("List documents/modules in configured project.")]
    public string ListDocuments(string? space = null, int limit = 200) =>
        Run("list_documents", new() { ["space"] = space, ["limit"] = limit });

    [McpServerTool(Name = "list_document_work_items"), Description("List work items in a document.")]
    public string ListDocumentWorkItems(string document_name, string? space = null, int limit = 200) =>
        Run(
            "list_document_work_items",
            new()
            {
                ["document_name"] = document_name,
                ["space"] = space,
                ["limit"] = limit
            }
        );

    [McpServerTool(Name = "list_configuration_srs_inventory"), Description("Inventory SRS/configuration mapping details.")]
    public string ListConfigurationSrsInventory(
        string? project_id = null,
        string? tool_configured_by = "Facility Config",
        string work_item_type = "config",
        int limit = 512
    ) =>
        Run(
            "list_configuration_srs_inventory",
            new()
            {
                ["project_id"] = project_id,
                ["tool_configured_by"] = tool_configured_by,
                ["work_item_type"] = work_item_type,
                ["limit"] = limit
            }
        );

    [McpServerTool(Name = "get_document_text"), Description("Get free text content from a document/module.")]
    public string GetDocumentText(string document_name, string? space = null, bool include_home_page_content = true) =>
        Run(
            "get_document_text",
            new()
            {
                ["document_name"] = document_name,
                ["space"] = space,
                ["include_home_page_content"] = include_home_page_content
            }
        );

    [McpServerTool(Name = "get_document_work_items"), Description("Legacy alias for list_document_work_items.")]
    public string GetDocumentWorkItems(string document_name, string? space = null, int limit = 200) =>
        Run(
            "get_document_work_items",
            new()
            {
                ["document_name"] = document_name,
                ["space"] = space,
                ["limit"] = limit
            }
        );

    [McpServerTool(Name = "list_work_item_comments"), Description("List comments on a work item.")]
    public string ListWorkItemComments(string work_item_id) => Run("list_work_item_comments", new() { ["work_item_id"] = work_item_id });

    [McpServerTool(Name = "add_work_item_comment"), Description("Add a comment to a work item.")]
    public string AddWorkItemComment(string work_item_id, string text, string mime_type = "text/plain", string? parent_comment_id = null) =>
        Run(
            "add_work_item_comment",
            new()
            {
                ["work_item_id"] = work_item_id,
                ["text"] = text,
                ["mime_type"] = mime_type,
                ["parent_comment_id"] = parent_comment_id
            }
        );

    [McpServerTool(Name = "list_reviewers"), Description("List reviewers and statuses on a work item.")]
    public string ListReviewers(string work_item_id) => Run("list_reviewers", new() { ["work_item_id"] = work_item_id });

    [McpServerTool(Name = "add_reviewer"), Description("Add a reviewer to a work item.")]
    public string AddReviewer(string work_item_id, string reviewer_id) =>
        Run("add_reviewer", new() { ["work_item_id"] = work_item_id, ["reviewer_id"] = reviewer_id });

    [McpServerTool(Name = "remove_reviewer"), Description("Remove a reviewer from a work item.")]
    public string RemoveReviewer(string work_item_id, string reviewer_id) =>
        Run("remove_reviewer", new() { ["work_item_id"] = work_item_id, ["reviewer_id"] = reviewer_id });

    [McpServerTool(Name = "reset_review_status"), Description("Reset review status for one or all reviewers.")]
    public string ResetReviewStatus(string work_item_id, string? reviewer_id = null) =>
        Run("reset_review_status", new() { ["work_item_id"] = work_item_id, ["reviewer_id"] = reviewer_id });

    [McpServerTool(Name = "create_test_result_record"), Description("Create a pass/fail test execution result.")]
    public string CreateTestResultRecord(
        string test_run_id,
        string test_case_id,
        bool passed,
        string? comment = null
    ) =>
        Run(
            "create_test_result_record",
            new()
            {
                ["test_run_id"] = test_run_id,
                ["test_case_id"] = test_case_id,
                ["passed"] = passed,
                ["comment"] = comment
            }
        );

    private string Run(string toolName, Dictionary<string, object?> arguments)
    {
        var result = _client.ExecuteTool(toolName, arguments);
        return JsonSerializer.Serialize(result, JsonOptions);
    }
}

using System.ComponentModel;
using GqpMcp.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace GqpMcp.Server;

[McpServerToolType]
public sealed class GqpTools
{
    private readonly GqpKnowledgeService _service;

    public GqpTools(GqpKnowledgeService service)
    {
        _service = service;
    }

    [McpServerTool(Name = "search_gqp_documents"), Description("Search GQP and GQT documents using hybrid semantic search.")]
    public Task<string> SearchGqpDocuments(
        string query,
        string doc_family = "",
        string doc_id = "",
        string doc_type = "",
        int top = GqpOptions.DefaultTopK) =>
        _service.SearchDocumentsAsync(query, doc_family, doc_id, doc_type, top);

    [McpServerTool(Name = "find_testing_requirements"), Description("Find verification and validation testing requirements for a GQP procedure.")]
    public Task<string> FindTestingRequirements(string procedure_id) =>
        _service.FindTestingRequirementsAsync(procedure_id, CancellationToken.None);

    [McpServerTool(Name = "find_risk_mitigations"), Description("Find risk management and mitigation tasks for a GQP procedure.")]
    public Task<string> FindRiskMitigations(string procedure_id) =>
        _service.FindRiskMitigationsAsync(procedure_id, CancellationToken.None);

    [McpServerTool(Name = "find_security_tasks"), Description("Find cybersecurity tasks and controls for a GQP procedure.")]
    public Task<string> FindSecurityTasks(string procedure_id) =>
        _service.FindSecurityTasksAsync(procedure_id, CancellationToken.None);

    [McpServerTool(Name = "find_required_deliverables"), Description("List required documents, records, and deliverables for a GQP procedure.")]
    public Task<string> FindRequiredDeliverables(string procedure_id) =>
        _service.FindRequiredDeliverablesAsync(procedure_id, CancellationToken.None);

    [McpServerTool(Name = "assess_change_impact"), Description("Identify GQP/GQT documents and activities affected by a proposed change.")]
    public Task<string> AssessChangeImpact(string change_description) =>
        _service.AssessChangeImpactAsync(change_description, CancellationToken.None);

    [McpServerTool(Name = "build_project_plan"), Description("Build a project plan with required GQP/GQT documentation for a development goal.")]
    public Task<string> BuildProjectPlan(string goal) =>
        _service.BuildProjectPlanAsync(goal, CancellationToken.None);
}

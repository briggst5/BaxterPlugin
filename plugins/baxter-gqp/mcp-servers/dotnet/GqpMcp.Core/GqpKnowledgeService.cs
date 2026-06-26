using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace GqpMcp.Core;

public sealed class GqpKnowledgeService
{
    private static readonly string[] SelectFields =
    [
        "id", "doc_id", "doc_family", "doc_type", "title", "filename",
        "section_heading", "content", "page_number", "revision",
        "parent_chunk_id", "chunk_level",
    ];

    private GqpOptions _options;
    private readonly TokenCredential _tokenCredential;
    private readonly ILogger<GqpKnowledgeService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _secretsLoaded;

    public GqpKnowledgeService(GqpOptions options, ILogger<GqpKnowledgeService> logger)
    {
        _options = options;
        _tokenCredential = GqpCredentialFactory.CreateRuntimeTokenCredential(options);
        _logger = logger;
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        if (_secretsLoaded)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_secretsLoaded)
            {
                return;
            }

            await WithTlsHelpAsync(async () =>
            {
                await GqpSecretsBootstrapper.EnsureSecretsAsync(
                    _options,
                    _tokenCredential,
                    _logger,
                    cancellationToken);
                return 0;
            });
            _options = GqpOptions.FromEnvironment();
            _secretsLoaded = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<T> WithTlsHelpAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (GqpHttpTransport.IsCertificateError(ex))
        {
            throw new InvalidOperationException(GqpHttpTransport.CertificateErrorHelp(_options), ex);
        }
    }

    /// <summary>
    /// Runs a backend operation with both TLS-error help and auth recovery. If the call is
    /// rejected for authentication/authorization (e.g. the cached Key Vault keys were rotated
    /// or revoked), the local secret cache is flushed and re-fetched from Key Vault - forcing
    /// a fresh sign-in when the Entra token is also gone - then the operation is retried once.
    /// </summary>
    private async Task<T> WithResilienceAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await WithTlsHelpAsync(action);
        }
        catch (Exception ex) when (IsAuthFailure(ex))
        {
            _logger.LogWarning(
                ex,
                "Backend rejected request ({Status}); flushing cached keys and forcing re-auth",
                AuthFailureStatus(ex));
            await ReloadSecretsAsync(CancellationToken.None);
            return await WithTlsHelpAsync(action);
        }
    }

    private async Task ReloadSecretsAsync(CancellationToken cancellationToken)
    {
        await _initLock.WaitAsync(cancellationToken);
        try
        {
            await WithTlsHelpAsync(async () =>
            {
                await GqpSecretsBootstrapper.EnsureSecretsAsync(
                    _options,
                    _tokenCredential,
                    _logger,
                    cancellationToken,
                    forceRefresh: true);
                return 0;
            });
            _options = GqpOptions.FromEnvironment();
            _secretsLoaded = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static bool IsAuthFailure(Exception ex) =>
        AuthFailureStatus(ex) is 401 or 403;

    private static int? AuthFailureStatus(Exception ex) => ex switch
    {
        Azure.RequestFailedException rfe => rfe.Status,
        System.ClientModel.ClientResultException cre => cre.Status,
        _ => null,
    };

    public async Task<string> SearchDocumentsAsync(
        string query,
        string docFamily = "",
        string docId = "",
        string docType = "",
        int top = GqpOptions.DefaultTopK,
        CancellationToken cancellationToken = default)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(docFamily))
        {
            parts.Add($"doc_family eq '{EscapeOData(docFamily)}'");
        }

        if (!string.IsNullOrWhiteSpace(docId))
        {
            parts.Add($"doc_id eq '{EscapeOData(docId)}'");
        }

        if (!string.IsNullOrWhiteSpace(docType))
        {
            parts.Add($"doc_type eq '{EscapeOData(docType)}'");
        }

        var filterExpr = parts.Count > 0 ? string.Join(" and ", parts) : null;
        var hits = await HybridSearchAsync(query, top, filterExpr, "child", cancellationToken);
        return FormatHits(hits);
    }

    public Task<string> FindTestingRequirementsAsync(string procedureId, CancellationToken cancellationToken) =>
        RunSynthesisAsync(
            query: "verification validation testing requirements acceptance criteria test protocol IQ OQ PQ design verification",
            procedureId: procedureId,
            systemPrompt: "You are a regulatory quality expert. Extract all verification and validation testing requirements from the provided GQP document excerpts. Format as a numbered list with the section reference and exact requirement text.",
            question: $"What are all V&V testing requirements defined in {procedureId}?",
            cancellationToken: cancellationToken);

    public Task<string> FindRiskMitigationsAsync(string procedureId, CancellationToken cancellationToken) =>
        RunSynthesisAsync(
            query: "risk management risk assessment FMEA DFMEA pFMEA hazard analysis risk mitigation risk control residual risk",
            procedureId: procedureId,
            systemPrompt: "You are a risk management expert. Extract all risk management activities, mitigations, and evidence requirements from the provided GQP document excerpts. Format as a numbered list with section references.",
            question: $"What risk management and mitigation tasks are required by {procedureId}?",
            cancellationToken: cancellationToken);

    public Task<string> FindSecurityTasksAsync(string procedureId, CancellationToken cancellationToken) =>
        RunSynthesisAsync(
            query: "cybersecurity security threat vulnerability SBOM software bill of materials penetration testing security testing FDA 524B IEC 81001-5-1 UL 2900",
            procedureId: procedureId,
            systemPrompt: "You are a medical device cybersecurity expert. Extract all cybersecurity tasks, controls, deliverables, and regulatory evidence requirements from the provided GQP document excerpts. Format as a numbered list with section references.",
            question: $"What cybersecurity tasks and controls are required by {procedureId}?",
            cancellationToken: cancellationToken);

    public Task<string> FindRequiredDeliverablesAsync(string procedureId, CancellationToken cancellationToken) =>
        RunSynthesisAsync(
            query: "required records deliverables outputs documents forms templates shall be documented shall be completed required evidence",
            procedureId: procedureId,
            systemPrompt: "You are a quality system expert. Extract every required deliverable, record, document, form, or template mentioned in the provided GQP excerpts. Group by phase or activity and include the document number (GQT/GQP) if stated.",
            question: $"What are all required deliverables and records for {procedureId}?",
            cancellationToken: cancellationToken);

    public async Task<string> AssessChangeImpactAsync(string changeDescription, CancellationToken cancellationToken)
    {
        var query = changeDescription + " change control design change impact assessment";
        var hits = await HybridSearchAsync(query, 12, filterExpr: null, chunkLevel: "parent", cancellationToken);
        var context = FormatHits(hits);
        return await SynthesizeAsync(
            "You are a senior medical device quality engineer. Given the change description and relevant GQP document excerpts, produce:\n1. A table of impacted GQP/GQT procedures with the specific section and activity.\n2. A bullet list of required deliverables/records.\n3. A recommended phase-gate sequence.\nBe specific with procedure numbers and section references.",
            context,
            $"Change description: {changeDescription}",
            cancellationToken);
    }

    public async Task<string> BuildProjectPlanAsync(string goal, CancellationToken cancellationToken)
    {
        var query = goal + " product development planning PDLM design controls deliverables";
        var hits = await HybridSearchAsync(query, 12, filterExpr: null, chunkLevel: "parent", cancellationToken);
        var context = FormatHits(hits);
        return await SynthesizeAsync(
            "You are an expert in Baxter's PDLM process and quality management system. Create a comprehensive project plan that includes:\n1. Development phases with gate criteria.\n2. Required GQP activities per phase (with procedure number and section).\n3. Required GQT templates and records per phase.\n4. Key risk management and V&V milestones.\n5. Regulatory submission considerations.\nFormat as a structured plan with phase headers and task tables.",
            context,
            $"Goal: {goal}",
            cancellationToken);
    }

    private async Task<string> RunSynthesisAsync(
        string query,
        string procedureId,
        string systemPrompt,
        string question,
        CancellationToken cancellationToken)
    {
        var filterExpr = $"doc_id eq '{EscapeOData(procedureId)}'";
        var hits = await HybridSearchAsync(query, GqpOptions.DefaultTopK, filterExpr, "child", cancellationToken);
        var context = FormatHits(hits);
        return await SynthesizeAsync(systemPrompt, context, question, cancellationToken);
    }

    private Task<IReadOnlyList<SearchHit>> HybridSearchAsync(
        string query,
        int top,
        string? filterExpr,
        string chunkLevel,
        CancellationToken cancellationToken) =>
        WithResilienceAsync(async () =>
        {
            await EnsureReadyAsync(cancellationToken);
            var searchClient = CreateSearchClient();
        var levelFilter = $"chunk_level eq '{EscapeOData(chunkLevel)}'";
        var combinedFilter = filterExpr is not null
            ? $"({levelFilter}) and ({filterExpr})"
            : levelFilter;

        var embedding = await EmbedAsync(query, cancellationToken);
        var vectorQuery = new VectorizedQuery(embedding)
        {
            KNearestNeighborsCount = top * 2,
            Fields = { "content_vector" },
        };

        var searchOptions = new SearchOptions
        {
            Filter = combinedFilter,
            QueryType = SearchQueryType.Semantic,
            Size = top,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = "default",
            },
        };

        foreach (var field in SelectFields)
        {
            searchOptions.Select.Add(field);
        }

        searchOptions.VectorSearch = new VectorSearchOptions();
        searchOptions.VectorSearch.Queries.Add(vectorQuery);

        var hits = new List<SearchHit>();
        var response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions, cancellationToken);
        await foreach (var result in response.Value.GetResultsAsync())
        {
            hits.Add(ToSearchHit(result.Document));
        }

        return (IReadOnlyList<SearchHit>)hits;
        });

    private Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken cancellationToken) =>
        WithResilienceAsync(async () =>
        {
            await EnsureReadyAsync(cancellationToken);
            var client = CreateOpenAiClient(_options.EmbedEndpoint, _options.OpenAiKey);
            var embeddingClient = client.GetEmbeddingClient(GqpOptions.EmbeddingDeployment);
            var response = await embeddingClient.GenerateEmbeddingsAsync([text], cancellationToken: cancellationToken);
            return response.Value[0].ToFloats();
        });

    private Task<string> SynthesizeAsync(
        string systemPrompt,
        string context,
        string question,
        CancellationToken cancellationToken) =>
        WithResilienceAsync(async () =>
        {
            await EnsureReadyAsync(cancellationToken);
            var client = CreateOpenAiClient(_options.GptEndpoint, _options.GptKey);
        var chatClient = client.GetChatClient(GqpOptions.GptDeployment);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"DOCUMENT CONTEXT:\n{context}\n\nQUESTION:\n{question}"),
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.1f,
            MaxOutputTokenCount = 2048,
        };

        var completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
        return completion.Value.Content.Count > 0
            ? completion.Value.Content[0].Text ?? string.Empty
            : string.Empty;
        });

    private SearchClient CreateSearchClient()
    {
        var clientOptions = new SearchClientOptions
        {
            Transport = GqpHttpTransport.CreateAzureTransport(_options),
        };

        if (!string.IsNullOrWhiteSpace(_options.SearchKey))
        {
            return new SearchClient(
                new Uri(_options.SearchEndpoint),
                GqpOptions.IndexName,
                new AzureKeyCredential(_options.SearchKey),
                clientOptions);
        }

        return new SearchClient(
            new Uri(_options.SearchEndpoint),
            GqpOptions.IndexName,
            _tokenCredential,
            clientOptions);
    }

    private AzureOpenAIClient CreateOpenAiClient(string endpoint, string? apiKey)
    {
        var baseUri = new Uri(endpoint.TrimEnd('/') + "/");
        var clientOptions = new AzureOpenAIClientOptions
        {
            Transport = GqpHttpTransport.CreateOpenAiTransport(_options),
        };

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return new AzureOpenAIClient(baseUri, new AzureKeyCredential(apiKey), clientOptions);
        }

        return new AzureOpenAIClient(baseUri, _tokenCredential, clientOptions);
    }

    private static SearchHit ToSearchHit(SearchDocument document)
    {
        return new SearchHit
        {
            DocId = GetDocumentString(document, "doc_id"),
            Revision = GetDocumentString(document, "revision"),
            Title = GetDocumentString(document, "title"),
            SectionHeading = GetDocumentString(document, "section_heading"),
            Content = GetDocumentString(document, "content"),
            PageNumber = GetDocumentString(document, "page_number"),
        };
    }

    private static string? GetDocumentString(SearchDocument document, string key)
    {
        if (!document.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            int i => i.ToString(),
            long l => l.ToString(),
            _ => value.ToString(),
        };
    }

    public static string FormatHits(IReadOnlyList<SearchHit> hits)
    {
        if (hits.Count == 0)
        {
            return "No results found.";
        }

        var lines = new List<string>();
        for (var i = 0; i < hits.Count; i++)
        {
            var hit = hits[i];
            var content = hit.Content ?? string.Empty;
            if (content.Length > 600)
            {
                content = content[..600];
            }

            lines.Add(
                $"[{i + 1}] {hit.DocId ?? string.Empty} Rev {hit.Revision ?? "?"} - {hit.Title ?? string.Empty} " +
                $"(p.{hit.PageNumber ?? "?"}) {hit.SectionHeading ?? string.Empty}\n{content}");
        }

        return string.Join("\n\n---\n\n", lines);
    }

    private static string EscapeOData(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}

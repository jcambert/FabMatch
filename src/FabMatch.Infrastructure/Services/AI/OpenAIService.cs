using System.Text;
using System.Text.Json;
using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace FabMatch.Infrastructure.Services.AI;

/// <summary>
/// AI service implementation backed by OpenAI's GPT-4o and text-embedding-3-small models.
/// Implements <see cref="IAIService"/> so it can be swapped out via DI configuration.
/// </summary>
public sealed class OpenAIService : IAIService
{
    private readonly OpenAIClient _client;
    private readonly OpenAIOptions _opts;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IOptions<OpenAIOptions> opts, ILogger<OpenAIService> logger)
    {
        _opts = opts.Value;
        _client = new OpenAIClient(_opts.ApiKey);
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProviderName => $"OpenAI {_opts.ChatModel}";

    /// <inheritdoc />
    public async Task<PlanAnalysisResult> AnalysePlanAsync(
        byte[] imageData,
        string contentType,
        string currency = "EUR",
        CancellationToken ct = default)
    {
        _logger.LogDebug("Analysing plan image ({ContentType}, {Bytes} bytes)", contentType, imageData.Length);

        var chatClient = _client.GetChatClient(_opts.ChatModel);

        var systemPrompt = """
            You are an expert manufacturing engineer specialising in sheet metal fabrication and CNC machining.
            Analyse the provided technical drawing and respond ONLY with a valid JSON object containing:
            {
              "identifiedProcesses": ["..."],
              "identifiedMaterials": ["..."],
              "estimatedManufacturingCost": 0.00,
              "estimatedSellingPrice": 0.00,
              "currency": "EUR",
              "estimatedDurationDays": 0,
              "confidenceScore": 0.0,
              "summary": "..."
            }
            All monetary amounts must be realistic estimates for European manufacturing.
            Confidence score must be between 0 and 1.
            """;

        var base64 = Convert.ToBase64String(imageData);
        var dataUrl = $"data:{contentType};base64,{base64}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart($"Analyse this technical drawing. Provide estimates in {currency}."),
                ChatMessageContentPart.CreateImagePart(new Uri(dataUrl), ChatImageDetailLevel.High))
        };

        var response = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
        var raw = response.Value.Content[0].Text;

        try
        {
            // Strip markdown fences if present
            var json = raw.Trim();
            if (json.StartsWith("```")) json = json[json.IndexOf('{')..json.LastIndexOf('}') + 1];

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new PlanAnalysisResult(
                IdentifiedProcesses: ReadStringArray(root, "identifiedProcesses"),
                IdentifiedMaterials: ReadStringArray(root, "identifiedMaterials"),
                EstimatedManufacturingCost: root.TryGetProperty("estimatedManufacturingCost", out var mc)
                    ? mc.GetDecimal() : null,
                EstimatedSellingPrice: root.TryGetProperty("estimatedSellingPrice", out var sp)
                    ? sp.GetDecimal() : null,
                Currency: root.TryGetProperty("currency", out var cur) ? cur.GetString()! : currency,
                EstimatedDurationDays: root.TryGetProperty("estimatedDurationDays", out var dur)
                    ? dur.GetInt32() : null,
                ConfidenceScore: root.TryGetProperty("confidenceScore", out var cs) ? cs.GetDouble() : 0.5,
                Summary: root.TryGetProperty("summary", out var s) ? s.GetString()! : "",
                RawResponse: raw);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response; returning fallback");
            return new PlanAnalysisResult([], [], null, null, currency, null, 0.3, "Analysis failed.", raw);
        }
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var embClient = _client.GetEmbeddingClient(_opts.EmbeddingModel);
        var response = await embClient.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return response.Value.ToFloats().ToArray();
    }

    /// <inheritdoc />
    public async Task<MatchScoreResult> ComputeMatchScoreAsync(
        string projectDescription,
        string supplierDescription,
        CancellationToken ct = default)
    {
        // For high-volume matching we use cosine similarity of embeddings (fast + cheap).
        // For detailed score+rationale, we also call GPT for the rationale text.
        var (projEmb, suppEmb) = await (
            GenerateEmbeddingAsync(projectDescription, ct),
            GenerateEmbeddingAsync(supplierDescription, ct));

        var score = CosineSimilarity(projEmb, suppEmb);

        // Only ask for rationale if there is a meaningful match
        string rationale;
        if (score >= 0.5)
        {
            var chatClient = _client.GetChatClient(_opts.ChatModel);
            var prompt = $"""
                Project: {projectDescription}
                Supplier: {supplierDescription}
                Affinity score: {score:F2}
                In one sentence, explain why this supplier is or is not a good match for the project.
                """;
            var response = await chatClient.CompleteChatAsync(
                [new UserChatMessage(prompt)], cancellationToken: ct);
            rationale = response.Value.Content[0].Text.Trim();
        }
        else
        {
            rationale = "Insufficient capability overlap for this project.";
        }

        return new MatchScoreResult(score, rationale);
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return normA == 0 || normB == 0 ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var arr)) return [];
        return arr.EnumerateArray()
            .Select(e => e.GetString() ?? "")
            .Where(s => s.Length > 0)
            .ToList();
    }
}

/// <summary>Configuration options for the OpenAI service.</summary>
public sealed class OpenAIOptions
{
    public const string Section = "AI:OpenAI";

    /// <summary>OpenAI API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Chat completion model (default: gpt-4o).</summary>
    public string ChatModel { get; set; } = "gpt-4o";

    /// <summary>Embedding model (default: text-embedding-3-small).</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}

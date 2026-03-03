using System.Net.Http.Json;
using System.Text.Json;
using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FabMatch.Infrastructure.Services.AI;

/// <summary>
/// Alternative AI service implementation backed by a local Ollama instance.
/// Implements <see cref="IAIService"/> and can be swapped in via appsettings.json.
/// </summary>
public sealed class OllamaAIService : IAIService
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _opts;
    private readonly ILogger<OllamaAIService> _logger;

    public OllamaAIService(
        HttpClient http,
        IOptions<OllamaOptions> opts,
        ILogger<OllamaAIService> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProviderName => $"Ollama {_opts.Model}";

    /// <inheritdoc />
    public async Task<PlanAnalysisResult> AnalysePlanAsync(
        byte[] imageData,
        string contentType,
        string currency = "EUR",
        CancellationToken ct = default)
    {
        _logger.LogDebug("Ollama: analysing plan image ({Bytes} bytes)", imageData.Length);

        var base64 = Convert.ToBase64String(imageData);
        var request = new
        {
            model = _opts.Model,
            prompt = $"Analyse this manufacturing drawing. Return a JSON object with identifiedProcesses, identifiedMaterials, estimatedManufacturingCost, estimatedSellingPrice, currency={currency}, estimatedDurationDays, confidenceScore (0-1), and summary.",
            images = new[] { base64 },
            stream = false
        };

        var response = await _http.PostAsJsonAsync("/api/generate", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
        var raw = result?.Response ?? "{}";

        try
        {
            var json = raw.Trim();
            if (json.Contains('{'))
                json = json[json.IndexOf('{')..(json.LastIndexOf('}') + 1)];

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new PlanAnalysisResult(
                IdentifiedProcesses: ReadStringArray(root, "identifiedProcesses"),
                IdentifiedMaterials: ReadStringArray(root, "identifiedMaterials"),
                EstimatedManufacturingCost: root.TryGetProperty("estimatedManufacturingCost", out var mc)
                    ? mc.GetDecimal() : null,
                EstimatedSellingPrice: root.TryGetProperty("estimatedSellingPrice", out var sp)
                    ? sp.GetDecimal() : null,
                Currency: currency,
                EstimatedDurationDays: root.TryGetProperty("estimatedDurationDays", out var dur)
                    ? dur.GetInt32() : null,
                ConfidenceScore: root.TryGetProperty("confidenceScore", out var cs)
                    ? cs.GetDouble() : 0.5,
                Summary: root.TryGetProperty("summary", out var s) ? s.GetString()! : "",
                RawResponse: raw);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama: failed to parse response");
            return new PlanAnalysisResult([], [], null, null, currency, null, 0.3, "Analysis failed.", raw);
        }
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var request = new { model = _opts.EmbeddingModel, prompt = text };
        var response = await _http.PostAsJsonAsync("/api/embeddings", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: ct);
        return result?.Embedding ?? [];
    }

    /// <inheritdoc />
    public async Task<MatchScoreResult> ComputeMatchScoreAsync(
        string projectDescription,
        string supplierDescription,
        CancellationToken ct = default)
    {
        var projectEmbeddingTask = GenerateEmbeddingAsync(projectDescription, ct);
        var supplierEmbeddingTask = GenerateEmbeddingAsync(supplierDescription, ct);
        await Task.WhenAll(projectEmbeddingTask, supplierEmbeddingTask);

        var projEmb = projectEmbeddingTask.Result;
        var suppEmb = supplierEmbeddingTask.Result;

        var score = CosineSimilarity(projEmb, suppEmb);
        return new MatchScoreResult(score, score >= 0.5 ? "Good capability overlap." : "Limited overlap.");
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length == 0 || a.Length != b.Length) return 0;
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

    // ── Internal DTOs ──────────────────────────────────────────────

    private sealed record OllamaResponse(string Response);
    private sealed record OllamaEmbeddingResponse(float[] Embedding);
}

/// <summary>Configuration options for the Ollama AI service.</summary>
public sealed class OllamaOptions
{
    public const string Section = "AI:Ollama";

    /// <summary>Base URL of the Ollama API (default: http://localhost:11434).</summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Vision/chat model name (e.g. "llava").</summary>
    public string Model { get; set; } = "llava";

    /// <summary>Embedding model name (e.g. "mxbai-embed-large").</summary>
    public string EmbeddingModel { get; set; } = "mxbai-embed-large";
}

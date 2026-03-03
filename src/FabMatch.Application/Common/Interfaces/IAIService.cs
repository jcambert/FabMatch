using FabMatch.Application.Common.Models;

namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the AI provider (OpenAI, Azure OpenAI, Ollama, etc.).
/// Swap implementations via DI configuration – no code changes required.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Analyses a plan image and returns manufacturing insights and cost estimates.
    /// </summary>
    /// <param name="imageData">Raw bytes of the plan image (PNG or JPEG).</param>
    /// <param name="contentType">MIME type, e.g. "image/png" or "image/jpeg".</param>
    /// <param name="currency">ISO 4217 currency code for cost estimates (default "EUR").</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PlanAnalysisResult> AnalysePlanAsync(
        byte[] imageData,
        string contentType,
        string currency = "EUR",
        CancellationToken ct = default);

    /// <summary>
    /// Generates a semantic embedding vector for a text description.
    /// Used to compute cosine-similarity between projects and suppliers.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Computes an affinity score between a project description and a supplier profile.
    /// Returns a value between 0 (no match) and 1 (perfect match).
    /// </summary>
    /// <param name="projectDescription">Text summary of the project requirements.</param>
    /// <param name="supplierDescription">Text summary of the supplier capabilities.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<MatchScoreResult> ComputeMatchScoreAsync(
        string projectDescription,
        string supplierDescription,
        CancellationToken ct = default);

    /// <summary>Name/identifier of the underlying AI provider (e.g. "OpenAI gpt-4o").</summary>
    string ProviderName { get; }
}

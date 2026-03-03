using FabMatch.Domain.Common;
using FabMatch.Domain.Enums;

namespace FabMatch.Domain.Entities;

/// <summary>
/// A technical drawing (blueprint) belonging to a <see cref="Project"/>.
/// Plans may be PNG, JPEG or PDF; PDFs are converted to images before AI analysis.
/// </summary>
public sealed class Plan : BaseEntity
{
    // ── Relations ──────────────────────────────────────────────────

    /// <summary>FK to the owning project.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Navigation to the owning project.</summary>
    public Project Project { get; private set; } = null!;

    // ── File metadata ──────────────────────────────────────────────

    /// <summary>Original filename as uploaded by the user.</summary>
    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>Detected / declared file format.</summary>
    public PlanFormat Format { get; private set; }

    /// <summary>
    /// Storage key (relative path or blob name) under which the file is stored.
    /// </summary>
    public string StorageKey { get; private set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; private set; }

    // ── Manufacturing context ──────────────────────────────────────

    /// <summary>Revision label (e.g. "Rev A", "v2.1").</summary>
    public string? Revision { get; private set; }

    /// <summary>
    /// Required manufacturing duration in working days.
    /// Provided by the client or estimated by AI analysis.
    /// </summary>
    public int? DurationDays { get; private set; }

    /// <summary>Number of parts/units required.</summary>
    public int? Quantity { get; private set; }

    /// <summary>Human-readable notes about this specific plan.</summary>
    public string? Notes { get; private set; }

    // ── AI analysis ────────────────────────────────────────────────

    /// <summary>Whether the AI analysis has been completed for this plan.</summary>
    public bool IsAnalysed { get; private set; }

    /// <summary>Latest analysis result linked to this plan (null before first analysis).</summary>
    public Analysis? LatestAnalysis { get; private set; }

    /// <summary>All historical analysis runs for this plan.</summary>
    public ICollection<Analysis> Analyses { get; private set; } = [];

    // ── Factory ────────────────────────────────────────────────────

    /// <summary>Creates a new plan entry after file upload.</summary>
    public static Plan Create(
        Guid projectId,
        string originalFileName,
        PlanFormat format,
        string storageKey,
        long fileSizeBytes,
        string? revision = null,
        int? durationDays = null,
        int? quantity = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        return new Plan
        {
            ProjectId = projectId,
            OriginalFileName = originalFileName,
            Format = format,
            StorageKey = storageKey,
            FileSizeBytes = fileSizeBytes,
            Revision = revision,
            DurationDays = durationDays,
            Quantity = quantity,
            Notes = notes
        };
    }

    /// <summary>Updates mutable plan properties.</summary>
    public void Update(
        string? revision,
        int? durationDays,
        int? quantity,
        string? notes)
    {
        Revision = revision;
        DurationDays = durationDays;
        Quantity = quantity;
        Notes = notes;
        Touch();
    }

    /// <summary>Marks the plan as analysed and links the latest analysis result.</summary>
    public void MarkAnalysed(Analysis analysis)
    {
        IsAnalysed = true;
        LatestAnalysis = analysis;
        Touch();
    }
}

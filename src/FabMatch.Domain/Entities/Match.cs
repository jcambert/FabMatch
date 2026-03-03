using FabMatch.Domain.Common;
using FabMatch.Domain.Enums;

namespace FabMatch.Domain.Entities;

/// <summary>
/// An AI-proposed match between a client <see cref="Project"/> and a <see cref="Supplier"/>.
/// The match lifecycle is tracked via <see cref="MatchStatus"/>.
/// </summary>
public sealed class Match : BaseEntity
{
    // ── Relations ──────────────────────────────────────────────────

    /// <summary>FK to the project for which the match was generated.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Navigation to the project.</summary>
    public Project Project { get; private set; } = null!;

    /// <summary>FK to the proposed supplier.</summary>
    public Guid SupplierId { get; private set; }

    /// <summary>Navigation to the supplier.</summary>
    public Supplier Supplier { get; private set; } = null!;

    // ── Scoring ────────────────────────────────────────────────────

    /// <summary>
    /// AI-computed affinity score between 0 (no match) and 1 (perfect match).
    /// Derived from cosine similarity of embedding vectors.
    /// </summary>
    public double AffinityScore { get; private set; }

    /// <summary>Human-readable AI explanation of why this supplier was matched.</summary>
    public string? MatchRationale { get; private set; }

    // ── Status ─────────────────────────────────────────────────────

    /// <summary>Current lifecycle status of the match.</summary>
    public MatchStatus Status { get; private set; } = MatchStatus.Proposed;

    // ── Timestamps ─────────────────────────────────────────────────

    /// <summary>UTC timestamp when the client acted on this match.</summary>
    public DateTime? ClientRespondedAt { get; private set; }

    /// <summary>UTC timestamp when the supplier acted on this match.</summary>
    public DateTime? SupplierRespondedAt { get; private set; }

    // ── Factory / transitions ──────────────────────────────────────

    /// <summary>Creates a new AI-proposed match.</summary>
    public static Match Create(
        Guid projectId,
        Guid supplierId,
        double affinityScore,
        string? matchRationale = null)
    {
        if (affinityScore is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(affinityScore), "Score must be between 0 and 1.");

        return new Match
        {
            ProjectId = projectId,
            SupplierId = supplierId,
            AffinityScore = affinityScore,
            MatchRationale = matchRationale
        };
    }

    /// <summary>Client accepts the proposed match.</summary>
    public void AcceptByClient()
    {
        Status = MatchStatus.AcceptedByClient;
        ClientRespondedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Client rejects the proposed match.</summary>
    public void RejectByClient()
    {
        Status = MatchStatus.RejectedByClient;
        ClientRespondedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Supplier accepts the match after client acceptance.</summary>
    public void AcceptBySupplier()
    {
        if (Status != MatchStatus.AcceptedByClient)
            throw new InvalidOperationException("Supplier can only accept after client acceptance.");
        Status = MatchStatus.AcceptedBySupplier;
        SupplierRespondedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Supplier rejects the match.</summary>
    public void RejectBySupplier()
    {
        Status = MatchStatus.RejectedBySupplier;
        SupplierRespondedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Finalises the match (contract awarded).</summary>
    public void Finalise()
    {
        if (Status != MatchStatus.AcceptedBySupplier)
            throw new InvalidOperationException("Match can only be finalised after mutual acceptance.");
        Status = MatchStatus.Finalised;
        Touch();
    }
}

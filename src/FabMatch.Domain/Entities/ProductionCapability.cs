using FabMatch.Domain.Common;

namespace FabMatch.Domain.Entities;

/// <summary>
/// Describes a single manufacturing process or machine owned by a <see cref="Supplier"/>.
/// Multiple capabilities form the supplier's production profile used for AI matching.
/// </summary>
public sealed class ProductionCapability : BaseEntity
{
    // ── Relations ──────────────────────────────────────────────────

    /// <summary>FK to the owning supplier.</summary>
    public Guid SupplierId { get; private set; }

    /// <summary>Navigation to the owning supplier.</summary>
    public Supplier Supplier { get; private set; } = null!;

    // ── Capability description ─────────────────────────────────────

    /// <summary>Category of process (e.g. "Laser Cutting", "Bending", "Welding", "CNC Milling").</summary>
    public string ProcessType { get; private set; } = string.Empty;

    /// <summary>Machine or equipment name/model (e.g. "Trumpf TruLaser 3030").</summary>
    public string? EquipmentName { get; private set; }

    /// <summary>Maximum sheet/part thickness in millimetres.</summary>
    public decimal? MaxThicknessMm { get; private set; }

    /// <summary>Maximum part dimension X in millimetres.</summary>
    public decimal? MaxDimensionXMm { get; private set; }

    /// <summary>Maximum part dimension Y in millimetres.</summary>
    public decimal? MaxDimensionYMm { get; private set; }

    /// <summary>Dimensional tolerance achievable in millimetres (e.g. ±0.1).</summary>
    public decimal? ToleranceMm { get; private set; }

    /// <summary>Additional free-text notes (surface treatment, finishes, etc.).</summary>
    public string? Notes { get; private set; }

    // ── Factory ────────────────────────────────────────────────────

    /// <summary>Creates a new production capability entry.</summary>
    public static ProductionCapability Create(
        Guid supplierId,
        string processType,
        string? equipmentName = null,
        decimal? maxThicknessMm = null,
        decimal? maxDimXMm = null,
        decimal? maxDimYMm = null,
        decimal? toleranceMm = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processType);
        return new ProductionCapability
        {
            SupplierId = supplierId,
            ProcessType = processType,
            EquipmentName = equipmentName,
            MaxThicknessMm = maxThicknessMm,
            MaxDimensionXMm = maxDimXMm,
            MaxDimensionYMm = maxDimYMm,
            ToleranceMm = toleranceMm,
            Notes = notes
        };
    }
}

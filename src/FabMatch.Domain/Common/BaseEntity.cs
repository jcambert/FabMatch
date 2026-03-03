namespace FabMatch.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides a strongly-typed primary key and audit fields.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier (GUID) for this entity.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>UTC timestamp of when the entity was created.</summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last update. Null if never updated.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Soft-delete flag. True means the entity is logically deleted.</summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>Marks the entity as deleted (soft delete).</summary>
    public void Delete()
    {
        IsDeleted = true;
        Touch();
    }

    /// <summary>Updates the <see cref="UpdatedAt"/> timestamp to now.</summary>
    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}

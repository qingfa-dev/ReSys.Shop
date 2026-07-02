namespace Shared.Application.Domain.Concerns.Auditable;

/// <summary>
/// Defines an entity with audit tracking capabilities for creation and modification metadata.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the timestamp when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    DateTimeOffset? ModifiedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    string? ModifiedBy { get; set; }
}
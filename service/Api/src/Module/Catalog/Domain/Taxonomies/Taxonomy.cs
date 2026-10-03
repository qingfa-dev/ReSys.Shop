using Module.Catalog.Domain.Taxons;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Taxonomies;

/// <summary>
/// Represents a taxonomy (classification tree) that organizes products into hierarchical taxons.
/// </summary>
// Invariant: Name != null && Name.Length <= 100; Position >= -1; Taxons is never null
public sealed partial class Taxonomy : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Presentation { get; set; } = string.Empty;
    public int Position { get; set; }
    #endregion Properties

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Relationships
    public ICollection<Taxon> Taxons { get; set; } = new List<Taxon>();
    #endregion Relationships

    #region Constructor
    internal Taxonomy() { }
    #endregion Constructor
}
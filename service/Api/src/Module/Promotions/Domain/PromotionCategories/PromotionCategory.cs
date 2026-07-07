using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Models;
using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Domain.PromotionCategories;

/// <summary>Represents a category for grouping promotions with common targeting rules.</summary>
// Invariant: Name is always set; Code is unique when present; IsDeleted governs soft-delete lifecycle
public sealed partial class PromotionCategory : AggregateRoot, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Presentation { get; set; }
    #endregion Properties

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal PromotionCategory() { }
    #endregion Constructor

    #region Relationships
    public ICollection<Promotion> Promotions { get; set; } = [];
    #endregion Relationships
}

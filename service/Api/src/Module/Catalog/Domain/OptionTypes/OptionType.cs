using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products.Options;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.OptionTypes;

/// <summary>
/// Represents an option type (e.g., Size, Color) that defines selectable product attributes.
/// </summary>
// Invariant: Name != null && Name.Length <= 100; Position >= -1; OptionValues is never null
public sealed partial class OptionType : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Presentation { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool Filterable { get; set; } // Indicate it show up in filter panel
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

    #region Navigation
    public ICollection<OptionValue> OptionValues { get; set; } = new List<OptionValue>();
    public ICollection<ProductOptionType> ProductOptionTypes { get; set; } = new List<ProductOptionType>();
    #endregion Navigation

    #region Constructor
    internal OptionType() { }
    #endregion Constructor
}
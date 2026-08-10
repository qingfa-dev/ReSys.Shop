using Module.Catalog.Domain.OptionTypes.Values;

using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Variants.Options;

/// <summary>
/// Represents the assignment of an option value to a specific product variant.
/// </summary>
// Invariant: VariantId != Guid.Empty; OptionValueId != Guid.Empty
public sealed partial class OptionValueVariant : Entity
{
    #region Relationships
    public Guid VariantId { get; set; }
    public Variant? Variant { get; set; }
    public Guid OptionValueId { get; set; }
    public OptionValue? OptionValue { get; set; }
    #endregion Relationships

    #region Constructor
    internal OptionValueVariant() { }
    #endregion Constructor
}
using Module.Catalog.Domain.OptionTypes;

using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products.Options;

/// <summary>
/// Represents the association between a product and an option type for variant configuration.
/// </summary>
// Invariant: ProductId != Guid.Empty; OptionTypeId != Guid.Empty
public partial class ProductOptionType : Entity
{
    #region Properties
    public int Position { get; set; }
    #endregion Properties

    #region Relationships
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid OptionTypeId { get; set; }
    public OptionType OptionType { get; set; } = null!;
    #endregion Relationships

    #region Constructor
    internal ProductOptionType() { }
    #endregion Constructor
}

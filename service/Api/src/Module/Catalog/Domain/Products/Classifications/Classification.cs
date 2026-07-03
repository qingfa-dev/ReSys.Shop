using Module.Catalog.Domain.Taxonomies.Taxons;

using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products.Classifications;

/// <summary>
/// Represents the assignment of a product to a taxon for categorization.
/// </summary>
// Invariant: ProductId is set when part of a valid product-taxon relationship
public partial class Classification : Entity
{
    #region Properties
    public int Position { get; set; }
    public bool IsAutomatic { get; set; }
    #endregion Properties

    #region Relationships
    public Guid? ProductId { get; set; }
    public Guid? TaxonId { get; set; }

    public Product? Product { get; set; }
    public Taxon? Taxon { get; set; }
    #endregion Relationships

    #region Constructor
    internal Classification() { }
    #endregion Constructor
}

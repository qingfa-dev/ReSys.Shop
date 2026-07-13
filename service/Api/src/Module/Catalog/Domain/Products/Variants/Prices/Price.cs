using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products.Variants.Prices;

/// <summary>
/// Represents a price point for a product variant, including sale pricing via compare-at amounts.
/// </summary>
// Invariant: Amount >= 0; Currency is ISO 4217 code; CompareAtAmount > Amount when IsOnSale
public sealed partial class Price : Entity
{
    #region Properties
    public decimal? Amount { get; set; }
    public decimal? CompareAtAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CountryIso { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? DeletedAt { get; set; }
    #endregion Properties

    #region Relationships
    public Guid? VariantId { get; set; }
    public Guid? PriceListId { get; set; }
    public Variant? Variant { get; set; }
    #endregion Relationships

    #region Constructor
    internal Price() { }
    #endregion Constructor

    #region Computed
    public bool IsOnSale => CompareAtAmount.HasValue && Amount.HasValue && CompareAtAmount > Amount;
    #endregion Computed
}
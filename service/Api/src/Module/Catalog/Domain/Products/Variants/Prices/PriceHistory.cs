using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products.Variants.Prices;

/// <summary>
/// Represents an audit record of a price change for a product variant.
/// </summary>
// Invariant: Amount >= 0; Currency != null; RecordedAt != default
public sealed partial class PriceHistory : Entity
{
    #region Properties
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset RecordedAt { get; set; }
    #endregion Properties

    #region Relationships
    public Guid PriceId { get; set; }
    public Guid VariantId { get; set; }
    public Price Price { get; set; } = null!;
    #endregion Relationships

    #region Constructor
    internal PriceHistory() { }
    #endregion Constructor
}
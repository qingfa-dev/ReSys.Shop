using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Options;
using Module.Catalog.Domain.Variants.Prices;
using Module.Customer.Domain.Wishlists.WishedItems;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockTransfers;
using Module.Ordering.Domain.LineItems;

using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Variants;

/// <summary>
/// Represents a specific variation of a product (e.g., a particular size and color combination).
/// </summary>
// @CAT-10 Invariant: IsMaster exclusive per Product; Sku unique; Price >= 0; OptionValues required for non-master; TrackInventory=true requires StockItem per location
public sealed partial class Variant : Entity, ISoftDeletable
{
    #region Properties
    public bool IsMaster { get; set; }
    public string? Sku { get; set; }
    public int Position { get; set; }
    public bool TrackInventory { get; set; } = VariantConstant.Defaults.TrackInventory;
    #endregion Properties

    #region Identifiers
    public string? Barcode { get; set; }
    public string? HsCode { get; set; }
    #endregion Identifiers

    #region Timestamp
    public DateTimeOffset? DiscontinuedOn { get; set; }
    #endregion Timestamp

    #region Physical Specs
    public decimal? Weight { get; set; } = VariantConstant.Defaults.Weight;
    public WeightUnit? WeightUnit { get; set; }

    public decimal? Height { get; set; } = VariantConstant.Defaults.Height;
    public decimal? Width { get; set; } = VariantConstant.Defaults.Width;
    public decimal? Depth { get; set; } = VariantConstant.Defaults.Depth;
    public DimensionUnit? DimensionsUnit { get; set; }
    #endregion Physical Specs

    #region Pricing
    public decimal? Price { get; set; } = VariantConstant.Defaults.Price;
    public decimal? CostPrice { get; set; } = VariantConstant.Defaults.CostPrice;
    public string? CostCurrency { get; set; } = VariantConstant.Defaults.CostCurrency;
    #endregion Pricing

    #region SoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion SoftDeletable

    #region Relationships
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public ICollection<Price> Prices { get; set; } = [];
    public ICollection<OptionValueVariant> OptionValueVariants { get; set; } = [];
    public ICollection<VariantImage> VariantImages { get; set; } = [];
    public ICollection<LineItem> LineItems { get; set; } = [];
    public ICollection<StockItem> StockItems { get; set; } = [];
    public ICollection<StockReservation> StockReservations { get; set; } = [];
    public ICollection<TransferItem> TransferItems { get; set; } = [];
    public ICollection<WishedItem> WishedItems { get; set; } = [];
    #endregion Relationships

    #region Constructor
    internal Variant() { } // For EF Core
    #endregion Constructor
}
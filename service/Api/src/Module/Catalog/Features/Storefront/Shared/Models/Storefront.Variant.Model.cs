using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Models;

public record StoreVariantStockInfo
{
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int TotalAvailable { get; init; }
    public bool Backorderable { get; init; }
    public List<StoreStockLocationInfo> Locations { get; init; } = [];
}

public record StoreStockLocationInfo
{
    public Guid StockLocationId { get; init; }
    public string? StockLocationName { get; init; } = string.Empty;
    public int CountOnHand { get; init; }
    public int ReservedCount { get; init; }
    public int AvailableCount { get; init; }
    public bool Backorderable { get; init; }
}

public record StoreProductVariantResponse : VariantListItemResponse
{
    #region Storefront
    public string? Currency { get; init; }
    public List<StoreOptionValueListItemResponse> OptionValues { get; init; } = [];
    public List<StoreVariantImageListItemResponse> Images { get; init; } = [];
    public List<StoreVariantPriceListItemRepsonse> Prices { get; init; } = [];
    public StoreVariantStockInfo Stock { get; init; } = new();
    #endregion
}

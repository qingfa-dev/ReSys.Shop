using Module.Catalog.Features.Admin.Variants.Shared.Models;
using Module.Catalog.Features.Storefront.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Models;

public record StoreVariantStockInfo
{
    #region Stock
    public int AvailableQuantity { get; init; }
    public bool Backorderable { get; init; }
    #endregion
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

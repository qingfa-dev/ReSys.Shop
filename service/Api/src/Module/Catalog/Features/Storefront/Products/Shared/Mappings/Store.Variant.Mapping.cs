using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Storefront.OptionTypes.Shared.Models;
using Module.Catalog.Features.Storefront.Products.Shared.Models;
using Module.Inventory.Services;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class StoreProductVariantMapping
{
    #region Variant

    private const string DefaultCurrency = "USD";

    public static T MapToStoreVariant<T>(this Variant variant) where T : StoreProductVariantResponse, new()
    {
        var defaultPrice = variant.Prices.FirstOrDefault();

        return new T
        {
            Id = variant.Id,
            Sku = variant.Sku ?? string.Empty,
            IsMaster = variant.IsMaster,

            #region Price Fallback
            Price = defaultPrice?.Amount ?? variant.Price ?? 0m,
            Currency = defaultPrice?.Currency ?? DefaultCurrency,
            #endregion Price Fallback

            OptionValues = variant.OptionValueVariants
                .Where(ov => ov.OptionValue is not null)
                .OrderBy(ov => ov.OptionValue!.OptionType?.Position)
                .Select(ov => new StoreOptionValueListItemResponse
                {
                    Id = ov.OptionValueId,
                    Name = ov.OptionValue!.Name,
                    Presentation = ov.OptionValue.Presentation,
                    Position = ov.OptionValue.Position,
                    OptionTypeId = ov.OptionValue.OptionTypeId,
                    OptionTypeName = ov.OptionValue.OptionType?.Name,
                })
                .ToList(),

            Images = variant.VariantImages
                .OrderBy(i => i.Position)
                .Select(i => i.MapToStoreListItem<StoreVariantImageListItemResponse>())
                .ToList(),

            Prices = variant.Prices
                .Select(p => p.MapToStoreItem<StoreVariantPriceListItemRepsonse>())
                .ToList(),
        };
    }

    #endregion



    #region Stock

    public static StoreVariantStockInfo MapToStockInfo(this VariantStockAvailability stock)
    {
        return new StoreVariantStockInfo
        {
            TotalOnHand = stock.TotalOnHand,
            TotalReserved = stock.TotalReserved,
            TotalAvailable = stock.TotalAvailable,
            Backorderable = stock.Backorderable,
            Locations = stock.Locations.Select(l => new StoreStockLocationInfo
            {
                StockLocationId = l.StockLocationId,
                StockLocationName = l.StockLocationName,
                CountOnHand = l.CountOnHand,
                ReservedCount = l.ReservedCount,
                AvailableCount = l.AvailableCount,
                Backorderable = l.Backorderable,
            }).ToList(),
        };
    }

    #endregion
}

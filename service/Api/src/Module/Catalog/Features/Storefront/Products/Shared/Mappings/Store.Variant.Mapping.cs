using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class StoreProductVariantMapping
{
    #region Variant

    private const string DefaultCurrency = "USD";

    public static StoreProductVariantResponse MapToStoreVariant(this Variant variant)
    {
        var defaultPrice = variant.Prices.FirstOrDefault();

        return new StoreProductVariantResponse
        {
            Id = variant.Id,
            Sku = variant.Sku,
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
                    VariantOptionValueId = ov.Id,
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
                .Select(i => i.MapToStoreImage())
                .ToList(),
        };
    }

    #endregion

    #region Images

    public static StoreVariantImageListItemResponse MapToStoreImage(this VariantImage image)
    {
        return new StoreVariantImageListItemResponse
        {
            Id = image.Id,
            Url = image.Url,
            Alt = image.Alt,
            Position = image.Position,
            ContentType = image.ContentType,
        };
    }

    #endregion

    #region Stock

    public static StoreVariantStockInfo MapToStockInfo(this (int Available, bool Backorderable) stock)
    {
        return new StoreVariantStockInfo
        {
            AvailableQuantity = stock.Available,
            Backorderable = stock.Backorderable,
        };
    }

    #endregion
}

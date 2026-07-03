using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants.Images;

using Module.Catalog.Features.Storefront.Products.Get.Search.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Search.Shared.Mappings;

public static class ProductSearchStoreMapping
{
    public static T MapToStoreSearch<T>(this Product entity, double? score = null) where T : StoreProductSearchResponse, new()
    {
        var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);
        var firstPrice = masterVariant?.Prices.FirstOrDefault();
        var firstImage = masterVariant?.VariantImages
            .Where(i => i.Type == VariantImageType.Default || i.Type == VariantImageType.Thumbnail)
            .MinBy(i => i.Position);

        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Slug = entity.Slug ?? string.Empty,
            Description = entity.Description,
            MinPrice = firstPrice?.Amount,
            Currency = firstPrice?.Currency,
            ThumbnailUrl = firstImage?.Url,
            ThumbnailAlt = firstImage?.Alt,
            Score = score,
        };
    }
}

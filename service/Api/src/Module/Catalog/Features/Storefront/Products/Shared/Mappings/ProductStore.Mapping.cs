using Module.Catalog.Domain.Products;

using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class ProductStoreMapping
{
    public static T MapToStoreDetail<T>(this Product entity) where T : StoreProductDetailResponse, new()
    {
        var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);

        var response = new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Slug = entity.Slug ?? string.Empty,
            Description = entity.Description,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            MasterVariantId = entity.MasterVariantId,
            MasterVariant = masterVariant?.MapToStoreVariant(),
            Variants = entity.Variants
                .Where(v => !v.IsDeleted)
                .Select(v => v.MapToStoreVariant())
                .ToList(),
            Properties = [],
            Taxons = entity.Classifications
                .Select(c => new StoreProductTaxonResponse
                {
                    Id = c.TaxonId == null ? Guid.Empty : c.TaxonId.Value,
                    Name = c.Taxon?.Name ?? string.Empty,
                    Permalink = c.Taxon?.Permalink ?? string.Empty,
                    Depth = c.Taxon?.Depth ?? 0,
                })
                .ToList(),
        };

        return response;
    }

    public static StoreProductVariantResponse MapToStoreVariant(this Variant variant)
    {
        var firstPrice = variant.Prices.FirstOrDefault();

        return new StoreProductVariantResponse
        {
            Id = variant.Id,
            Sku = variant.Sku,
            IsMaster = variant.IsMaster,
            Price = firstPrice?.Amount,
            Currency = firstPrice?.Currency,
            Images = variant.VariantImages
                .OrderBy(i => i.Position)
                .Select(i => i.MapToStoreImage())
                .ToList(),
        };
    }

    public static StoreProductImageResponse MapToStoreImage(this VariantImage image)
    {
        return new StoreProductImageResponse
        {
            Id = image.Id,
            Url = image.Url,
            Alt = image.Alt,
            Position = image.Position,
            ContentType = image.ContentType,
        };
    }

    public static T MapToStoreListItem<T>(this Product entity) where T : StoreProductListItemResponse, new()
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
            AvailableOn = entity.AvailableOn,
            VariantsCount = entity.Variants.Count(v => !v.IsDeleted),
        };
    }
}

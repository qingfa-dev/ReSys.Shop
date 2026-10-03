using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Mappings;

public static class StoreProductMapping
{
    #region Detail

    public static T MapToStoreDetail<T>(this Product entity) where T : StoreProductDetailResponse, new()
    {
        var response = entity.MapToDetail<T>();
        var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);
        var taxons = entity.Classifications?
            .Select(c => c.Taxon)
            .Where(t => t is not null)
            .Select(t => t!.MapToStoreListItem<StoreTaxonListItemResponse>())
            .ToList() ?? [];

        return response with
        {
            MasterVariant = masterVariant?.MapToStoreVariant<StoreProductVariantResponse>(),
            Variants = entity.Variants
                .Where(v => !v.IsDeleted)
                .Select(v => v.MapToStoreVariant<StoreProductVariantResponse>())
                .ToList(),
            Classifications = taxons,
            VariantsCount = entity.Variants.Count,
        };
    }

    #endregion

    #region List Item

    public static T MapToStoreListItem<T>(this Product entity) where T : StoreProductListItemResponse, new()
    {
        var response = entity.MapToListItem<T>();
        var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);
        var taxons = entity.Classifications?
            .Select(c => c.Taxon)
            .Where(t => t is not null)
            .Select(t => t!.MapToStoreListItem<StoreTaxonListItemResponse>())
            .ToList() ?? [];

        return response with
        {
            MasterVariant = masterVariant?.MapToStoreVariant<StoreProductVariantResponse>(),
            Classifications = taxons,
        };
    }

    #endregion
}

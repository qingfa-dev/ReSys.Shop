using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Classifications.Shared.Mappings;
using Module.Catalog.Features.Storefront.Classifications.Shared.Models;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

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
            MasterVariant = masterVariant?.MapToStoreVariant(),
            Variants = entity.Variants
                .Where(v => !v.IsDeleted)
                .Select(v => v.MapToStoreVariant())
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
            MasterVariant = masterVariant?.MapToStoreVariant(),
            Classifications = taxons,
        };
    }

    #endregion
}

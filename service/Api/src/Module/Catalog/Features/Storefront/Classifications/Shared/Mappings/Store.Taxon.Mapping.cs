using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Shared.Mappings;
using Module.Catalog.Features.Storefront.Classifications.Shared.Models;

namespace Module.Catalog.Features.Storefront.Classifications.Shared.Mappings;

public static class StoreTaxonMapping
{
    public static T MapToStoreListItem<T>(this Taxon entity) where T : StoreTaxonListItemResponse, new()
    {
        var baseResponse = entity.MapToListItem<T>();
        return baseResponse;
    }
}
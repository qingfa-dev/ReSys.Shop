using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Mappings;

public static class StoreTaxonMapping
{
    public static T MapToStoreListItem<T>(this Taxon entity) where T : StoreTaxonListItemResponse, new()
    {
        var baseResponse = entity.MapToListItem<T>();
        return baseResponse;
    }
}

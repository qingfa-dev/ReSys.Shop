using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;
using Module.Catalog.Features.Storefront.Classifications.Shared.Models;

namespace Module.Catalog.Features.Storefront.Classifications.Shared.Mappings;

public static class StoreTaxonomyMapping
{
    public static T MapToStoreListItem<T>(this Taxonomy entity) where T : StoreTaxonomyListItemResponse, new()
    {
        var baseResponse = entity.MapToListItem<T>();
        return baseResponse;
    }
}
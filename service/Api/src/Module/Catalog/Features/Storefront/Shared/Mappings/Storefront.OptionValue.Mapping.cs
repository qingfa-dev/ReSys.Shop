using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Mappings;

public static class StoreOptionValueMappings
{
    public static T MapToStoreListItem<T>(this OptionValue entity) where T : StoreOptionValueListItemResponse, new()
    {
        return entity.MapToDetail<T>();
    }
}

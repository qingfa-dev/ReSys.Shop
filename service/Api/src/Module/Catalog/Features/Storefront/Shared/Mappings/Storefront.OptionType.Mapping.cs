using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Mappings;

public static class StoreOptionTypeMappings
{
    public static T MapToStoreListItem<T>(this OptionType entity) where T : StoreOptionTypeListItem, new()
    {
        return entity.MapToDetail<T>(); ;
    }
}

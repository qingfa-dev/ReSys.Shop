using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;
using Module.Catalog.Features.Storefront.Options.Shared.Models;

namespace Module.Catalog.Features.Storefront.Options.Shared.Mappings;

public static class StoreOptionTypeMappings
{
    public static T MapToStoreListItem<T>(this OptionType entity) where T : StoreOptionTypeListItem, new()
    {
        return entity.MapToDetail<T>(); ;
    }
}
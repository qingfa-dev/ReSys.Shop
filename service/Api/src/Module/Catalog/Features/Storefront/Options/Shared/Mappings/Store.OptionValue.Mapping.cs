using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;
using Module.Catalog.Features.Storefront.Options.Shared.Models;

namespace Module.Catalog.Features.Storefront.Options.Shared.Mappings;

public static class StoreOptionValueMappings
{
    public static T MapToStoreListItem<T>(this OptionValue entity) where T : StoreOptionValueListItemResponse, new()
    {
        return entity.MapToDetail<T>();
    }
}
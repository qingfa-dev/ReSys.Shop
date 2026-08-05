using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;
using Module.Catalog.Features.Storefront.Options.Shared.Models;

namespace Module.Catalog.Features.Storefront.Options.Shared.Mappings;

public static class StoreOptionTypeMappings
{
    public static T MapToStoreResponse<T>(this OptionType entity) where T : StoreOptionTypeResponse, new()
    {
        return entity.MapToDetail<T>(); ;
    }

}
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

public static partial class StockLocationMapping
{
    // Map: Request -> Domain entity (create)
    public static Result<StockLocation> MapToDomain<T>(this T request) where T : StockLocationRequest
    {
        return StockLocationMethod.Create(
            name: request.Name,
            active: request.Active,
            isDefault: request.Default,
            presentation: request.Presentation,
            code: request.Code,
            address1: request.Address1,
            address2: request.Address2,
            city: request.City,
            postalCode: request.PostalCode,
            phone: request.Phone,
            backorderableDefault: request.BackorderableDefault,
            propagateAllVariants: request.PropagateAllVariants,
            position: request.Position);
    }

    // Map: Request -> Domain entity (update)
    public static Result MapToDomain<T>(this T request, StockLocation location) where T : StockLocationRequest
    {
        return location.Update(
            name: request.Name,
            presentation: request.Presentation,
            code: request.Code,
            address1: request.Address1,
            address2: request.Address2,
            city: request.City,
            postalCode: request.PostalCode,
            phone: request.Phone,
            active: request.Active,
            isDefault: request.Default,
            backorderableDefault: request.BackorderableDefault,
            propagateAllVariants: request.PropagateAllVariants,
            position: request.Position);
    }
}

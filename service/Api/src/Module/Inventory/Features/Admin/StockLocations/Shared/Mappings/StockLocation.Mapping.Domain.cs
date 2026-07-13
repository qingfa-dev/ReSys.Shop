using Shared.Application.Domain.Concerns.Auditable;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

public static partial class StockLocationMapping
{
    public static Result<StockLocation> MapToDomain<T>(this T request) where T : StockLocationRequest
    {
        var result = StockLocationMethod.Create(
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

        if (result.IsSuccess)
        {
            AuditableBehavior.Create(result.Value);
        }

        return result;
    }

    public static Result MapToDomain<T>(this T request, StockLocation location) where T : StockLocationRequest
    {
        var result = location.Update(
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

        if (result.IsSuccess)
        {
            AuditableBehavior.Touch(location);
        }

        return result;
    }
}
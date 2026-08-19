using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.Shared.Models;
using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Inventory.Features.Admin.Shared.Mappings;

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

public static partial class StockLocationMapping
{
    public static T MapToDetail<T>(this StockLocation entity) where T : StockLocationDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name,
            Presentation = entity.Presentation,
            Code = entity.Code,
            Address1 = entity.Address1,
            Address2 = entity.Address2,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Phone = entity.Phone,
            Active = entity.Active,
            Default = entity.Default,
            BackorderableDefault = entity.BackorderableDefault,
            PropagateAllVariants = entity.PropagateAllVariants,
            Position = entity.Position,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this StockLocation entity) where T : StockLocationListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name,
            Presentation = entity.Presentation,
            Code = entity.Code,
            Address1 = entity.Address1,
            Address2 = entity.Address2,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Phone = entity.Phone,
            Active = entity.Active,
            Default = entity.Default,
            BackorderableDefault = entity.BackorderableDefault,
            PropagateAllVariants = entity.PropagateAllVariants,
            Position = entity.Position,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }
}

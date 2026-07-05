using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

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

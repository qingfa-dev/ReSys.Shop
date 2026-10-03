using Module.Location.Domain.Countries;
using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Shared.Countries.Mappings;

public static partial class CountryMapping
{

    public static T MapToDetail<T>(this Country country) where T : CountryDetailResponse, new()
    {
        return new T
        {
            Id = country.Id,
            Name = country.Name,
            IsoCode = country.IsoCode,
            CallingCode = country.CallingCode,
            StatesRequired = country.StatesRequired,
            IsActive = country.IsActive,
            CreatedAtUtc = country.CreatedAtUtc,
            ModifiedAtUtc = country.ModifiedAtUtc,
            CreatedBy = country.CreatedBy,
            ModifiedBy = country.ModifiedBy
        };
    }

    public static T MapToListItem<T>(this Country country) where T : CountryListItemResponse, new()
    {
        return new T
        {
            Id = country.Id,
            Name = country.Name,
            IsoCode = country.IsoCode,
            CallingCode = country.CallingCode,
            StatesRequired = country.StatesRequired,
            IsActive = country.IsActive
        };
    }
}
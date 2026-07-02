using Shared.Application.Domain.Concerns.Auditable;

using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Module.Location.Features.Admin.Countries.Shared.Mappings;

public static partial class CountryMapping
{
    // Entity:

    public static Country MapToDomain<T>(this T request) where T : CountryRequest
    {
        var country = new Country
        {
            Name = request.Name,
            IsoCode = request.IsoCode,
            CallingCode = request.CallingCode,
            StatesRequired = request.StatesRequired,
            IsActive = request.IsActive,
        };

        AuditableBehavior.Create(entity: country);
        return country;
    }

    public static void MapToDomain<T>(this T request, Country country) where T : CountryRequest
    {
        country.Name = request.Name;
        country.IsoCode = request.IsoCode;
        country.CallingCode = request.CallingCode;
        country.StatesRequired = request.StatesRequired;
        country.IsActive = request.IsActive;

        AuditableBehavior.Touch(entity: country);
    }
}
using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Admin.Countries.GetByIsoCode;

public static partial class GetCountryByIso
{
    public record Response : CountryDetailResponse;
}
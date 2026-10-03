using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Admin.Countries.GetById;

public static partial class GetCountryById
{
    public record Response : CountryDetailResponse;
}
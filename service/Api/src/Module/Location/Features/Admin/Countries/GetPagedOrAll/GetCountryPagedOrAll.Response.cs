using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Admin.Countries.GetPagedOrAll;

public static partial class GetCountryPagedOrAll
{
    // ============ RESPONSE ============
    public record Response : CountryListItemResponse;
}
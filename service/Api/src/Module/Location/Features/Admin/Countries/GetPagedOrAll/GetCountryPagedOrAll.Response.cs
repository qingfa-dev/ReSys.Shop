using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Module.Location.Features.Admin.Countries.GetPagedOrAll;

public static partial class GetCountryPagedOrAll
{
    // ============ RESPONSE ============
    public record Response : CountryListItemResponse;
}
using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Module.Location.Features.Admin.Countries.Update;

public static partial class UpdateCountry
{
    // ============ RESPONSE ============
    public record Response : CountryDetailResponse;
}
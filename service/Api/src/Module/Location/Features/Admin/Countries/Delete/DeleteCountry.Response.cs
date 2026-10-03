using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Admin.Countries.Delete;

public static partial class DeleteCountry
{
    // ============ RESPONSE ============
    public record Response : CountryListItemResponse;
}
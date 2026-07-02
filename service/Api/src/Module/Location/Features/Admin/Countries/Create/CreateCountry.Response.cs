using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Module.Location.Features.Admin.Countries.Create;

public static partial class CreateCountry
{
    // Response
    public record Response : CountryListItemResponse;
}
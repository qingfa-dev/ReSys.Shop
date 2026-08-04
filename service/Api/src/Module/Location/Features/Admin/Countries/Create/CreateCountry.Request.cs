using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Admin.Countries.Create;

public static partial class CreateCountry
{
    // Request
    public record Request : CountryRequest;
}
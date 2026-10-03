using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Admin.Countries.Update;

public static partial class UpdateCountry
{
    // ============ REQUEST ============
    public record Request : CountryRequest;
}
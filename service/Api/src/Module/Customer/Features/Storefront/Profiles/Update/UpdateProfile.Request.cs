using Module.Customer.Features.Shared.Profiles.Models;

namespace Module.Customer.Features.Storefront.Profiles.Update;

public static partial class UpdateProfile
{
    /// <summary>
    /// Represents the request contract for updating profile fields.
    /// </summary>
    public record Request : ProfileRequest
    {
        public string? Password { get; init; }
    }
}
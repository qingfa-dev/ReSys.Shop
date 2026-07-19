using Module.Profile.Features.Admin.Profiles.Shared.Models;

namespace Module.Profile.Features.Store.Profiles.Update;

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
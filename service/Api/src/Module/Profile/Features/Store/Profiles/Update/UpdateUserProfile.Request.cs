using Module.Profile.Features.Store.Profiles.Shared.Models;

namespace Module.Profile.Features.Store.Profiles.Update;

public static partial class UpdateUserProfile
{
    public class Request : ProfileRequest
    {
        public string? Password { get; init; }
    }
}

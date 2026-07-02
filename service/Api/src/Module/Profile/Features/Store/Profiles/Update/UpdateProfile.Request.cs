using Module.Profile.Features.Store.Profile.Shared.Models;

namespace Module.Profile.Features.Store.Profile.Update;

public static partial class UpdateProfile
{
    /// <summary>
    /// Represents the request contract for updating profile fields.
    /// </summary>
    public class Request : ProfileRequest
    {
        /// <summary>
        /// Gets or initializes the password required for sensitive changes.
        /// </summary>
        public string? Password { get; init; }
    }
}

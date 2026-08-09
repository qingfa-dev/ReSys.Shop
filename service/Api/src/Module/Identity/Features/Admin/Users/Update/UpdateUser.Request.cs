using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Update;

public static partial class UpdateUser
{
    /// <summary>
    /// Represents the request contract for updating an existing user.
    /// </summary>
    public record Request : UserRequest
    {
    }
}
using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Admin.Users.Update;

public static partial class UpdateUser
{
    /// <summary>
    /// Represents the request contract for updating an existing user.
    /// </summary>
    public record Request : UserRequest
    {
        public required Guid Id { get; init; }
    }
}
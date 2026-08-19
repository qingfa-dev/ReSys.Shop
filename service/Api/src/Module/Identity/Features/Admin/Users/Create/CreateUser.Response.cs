using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Create;

public static partial class CreateUser
{
    /// <summary>
    /// Represents the response contract for a created user.
    /// Inherits properties from <see cref="UserDetailResponse"/>.
    /// </summary>
    public record Response : UserDetailResponse;
}
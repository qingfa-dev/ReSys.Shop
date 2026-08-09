using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Update;

public static partial class UpdateUser
{
    /// <summary>
    /// Represents the response contract for an updated user.
    /// Inherits properties from <see cref="UserDetailResponse"/>.
    /// </summary>
    public record Response : UserDetailResponse;
}
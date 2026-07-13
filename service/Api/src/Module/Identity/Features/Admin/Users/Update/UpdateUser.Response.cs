using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Admin.Users.Update;

public static partial class UpdateUser
{
    /// <summary>
    /// Represents the response contract for an updated user.
    /// Inherits properties from <see cref="UserDetailResponse"/>.
    /// </summary>
    public class Response : UserDetailResponse { }
}
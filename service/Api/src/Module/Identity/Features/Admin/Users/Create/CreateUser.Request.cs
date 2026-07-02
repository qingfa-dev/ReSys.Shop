using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Admin.Users.Create;

public static partial class CreateUser
{
    /// <summary>
    /// Represents the request contract for creating a new user.
    /// Inherits properties from <see cref="UserRequest"/>.
    /// </summary>
    public class Request : UserRequest { }
}

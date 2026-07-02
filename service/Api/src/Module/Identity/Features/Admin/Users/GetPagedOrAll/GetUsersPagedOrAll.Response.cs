using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    /// <summary>
    /// Represents the response contract for a list of users, typically used in paged results.
    /// </summary>
    public class Response : UserListResponse { }
}

namespace Module.Identity.Features.Admin.Users.Delete;

public static partial class DeleteUser
{
    /// <summary>
    /// Represents the response contract for a deleted user.
    /// </summary>
    /// <param name="Id">The unique identifier of the deleted user.</param>
    public sealed record Response(Guid Id);
}
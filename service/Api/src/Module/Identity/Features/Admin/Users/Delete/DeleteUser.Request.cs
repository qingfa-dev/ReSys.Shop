namespace Module.Identity.Features.Admin.Users.Delete;

public static partial class DeleteUser
{
    /// <summary>
    /// Represents the request contract for deleting a user.
    /// </summary>
    public record Request
    {
        /// <summary>
        /// Gets or initializes the unique identifier of the user to be deleted.
        /// </summary>
        public Guid Id { get; init; }
    }
}
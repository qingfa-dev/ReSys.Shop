using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Profile.Create;

/// <summary>
/// Defines the use case for creating a new profile for the authenticated user.
/// </summary>
public static partial class CreateProfile
{
    /// <summary>
    /// Represents the command to create a new profile.
    /// </summary>
    /// <param name="UserId">The unique identifier of the user.</param>
    /// <param name="Request">The request containing profile details.</param>
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles the <see cref="Command"/> to create a new user profile.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the creation of a new profile for an existing identity user.
        /// Validates user existence, enforces one-profile-per-user, and persists via shared mapping.
        /// </summary>
        /// <param name="command">The command containing the user ID and profile data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the created profile details or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Target user must exist in identity store before profile creation.
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            // Check: Enforce one profile per user — duplicate detection.
            var existing = await dbContext.Set<UserProfile>()
                .AnyAsync(p => p.UserId == command.UserId, cancellationToken);

            if (existing)
                return UserProfileResult.Failure.AlreadyExists;

            // Create: Build UserProfile from validated request via shared domain mapping.
            var profile = request.MapToDomain();
            profile.UserId = user.Id;

            dbContext.Set<UserProfile>().Add(profile);
            await dbContext.SaveChangesAsync(cancellationToken);
            return profile.MapToDetail<Response>();
        }
    }
}
using Module.Profile.Domain;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Profile.Delete;

/// <summary>
/// Defines the use case for deactivating the authenticated user's profile.
/// </summary>
public static partial class DeleteProfile
{
    /// <summary>
    /// Represents the command to deactivate the authenticated user's profile.
    /// </summary>
    public sealed record Command(Guid UserId) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to soft-deactivate the authenticated user's profile.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the soft-deactivation of the profile for the specified user.
        /// Sets IsActive to false and records the modification timestamp.
        /// </summary>
        /// <param name="request">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A success result or an error.</returns>
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            profile.IsActive = false;
            AuditableBehavior.Touch(profile);

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
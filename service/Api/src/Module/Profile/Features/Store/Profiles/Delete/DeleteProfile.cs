using Module.Profile.Domain;

using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Profile.Features.Store.Profiles.Delete;

/// <summary>Soft-deactivates the specified user's profile by marking it inactive.</summary>
public static partial class DeleteProfile
{
    public sealed record Command(Guid UserId) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Sets IsActive to false and records the modification timestamp.</summary>
        /// <param name="request">The command containing the user ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or a not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            profile.IsActive = false;
            AuditableBehavior.Touch(profile);

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
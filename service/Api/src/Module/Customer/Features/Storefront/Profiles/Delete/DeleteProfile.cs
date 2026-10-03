using Module.Customer.Domain;

using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Customer.Features.Storefront.Profiles.Delete;

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
            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            // Validate: Confirm profile exists before deactivation
            if (profile is null)
                return UserProfileResult.Failure.NotFound;

            // Update: Soft-deactivate the profile
            profile.IsActive = false;
            AuditableBehavior.Touch(profile);

            // Call: Persist deactivation to the database
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
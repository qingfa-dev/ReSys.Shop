using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.Shared.Mappings;

namespace Module.Profile.Features.Storefront.Profiles.Update;

/// <summary>Updates or creates the authenticated user's profile fields.</summary>
public static partial class UpdateProfile
{
    /// <param name="UserId">The unique identifier of the user whose profile to update.</param>
    /// <param name="Request">The request containing updated profile details.</param>
    /// <param name="IsAdminBypass">When true, skips ownership check for admin-initiated operations.</param>
    public sealed record Command(Guid UserId, Request Request, bool IsAdminBypass = false) : ICommand<Response>;

    /// <summary>Handles the update of the current user's profile.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates the current user's profile.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Extract and prepare input data
            var request = command.Request;
            var userId = command.UserId;

            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            // Transform: Apply request fields to existing profile or create new one
            if (profile is not null)
            {
                request.MapToDomain(profile);
            }
            else
            {
                // Create: Build new profile entity from request
                profile = request.MapToDomain();
                profile.UserId = userId;
                dbContext.Set<UserProfile>().Add(profile);
            }

            // Call: Persist profile changes to the database
            await dbContext.SaveChangesAsync(cancellationToken);
            // Transform: Map updated profile to response DTO
            return profile.MapToDetail<Response>();
        }
    }
}

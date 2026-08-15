using Module.Customer.Domain;
using Module.Customer.Features.Shared.Profiles.Mappings;

namespace Module.Customer.Features.Storefront.Profiles.Update;

/// <summary>Updates or creates the authenticated user's profile fields.</summary>
public static partial class UpdateProfile
{
    public sealed record Command(Parameters Parameters) : ICommand<Response>;

    /// <summary>Handles the update of the current user's profile.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates the current user's profile.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Extract and prepare input data
            var request = command.Parameters.Request;
            var userId = command.Parameters.UserId;

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

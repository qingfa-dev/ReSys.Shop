using Module.Profile.Domain;
using Module.Profile.Features.Store.Profiles.Shared.Mappings;

namespace Module.Profile.Features.Store.Profiles.Update;

/// <summary>Updates or creates the authenticated user's profile fields.</summary>
public static partial class UpdateProfile
{
    /// <param name="UserId">The unique identifier of the user whose profile to update.</param>
    /// <param name="Request">The request containing updated profile details.</param>
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Applies partial updates to the profile, creating one if it does not exist.</summary>
        /// <param name="command">The command containing the user ID and profile update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated profile details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var userId = command.UserId;

            if (!Guid.TryParse(currentUser.UserId, out var currentUserId) || userId != currentUserId)
                return UserProfileResult.Failure.AuthRequired;

            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            var firstNameUpdated = false;
            var lastNameUpdated = false;
            var dateOfBirthUpdated = false;

            if (profile is not null)
            {
                firstNameUpdated = request.FirstName != profile.FirstName;
                lastNameUpdated = request.LastName != profile.LastName;
                dateOfBirthUpdated = request.DateOfBirth != profile.DateOfBirth;

                request.MapToDomain(profile);
            }
            else
            {
                firstNameUpdated = true;
                lastNameUpdated = true;

                profile = request.MapToDomain();
                profile.UserId = userId;
                dbContext.Set<UserProfile>().Add(profile);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            var response = profile.MapToDetail<Response>();
            return response;
        }
    }
}

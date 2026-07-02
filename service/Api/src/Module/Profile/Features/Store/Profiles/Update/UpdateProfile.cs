using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Shared.Mappings;

namespace Module.Profile.Features.Store.Profile.Update;

/// <summary>
/// Defines the use case for updating the authenticated user's profile.
/// </summary>
public static partial class UpdateProfile
{
    /// <summary>
    /// Represents the command to update profile fields.
    /// </summary>
    /// <param name="UserId">The unique identifier of the user whose profile to update.</param>
    /// <param name="Request">The request containing updated profile details.</param>
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles the <see cref="Command"/> to update profile fields and synchronize UserProfile.
    /// </summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the update of profile fields on the identity User
        /// and synchronizes changes to the associated UserProfile entity.
        /// </summary>
        /// <param name="command">The command containing updated profile data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing updated profile details with change-tracking flags or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var userId = command.UserId;

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

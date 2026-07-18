using Module.Profile.Domain;
using Module.Profile.Features.Store.Profiles.Shared.Mappings;

namespace Module.Profile.Features.Store.Profiles.Update;

public static partial class UpdateUserProfile
{
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
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

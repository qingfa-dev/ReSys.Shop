using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Admin.Profiles.CreateUserProfile;

public static partial class CreateUserProfile
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            var existing = await dbContext.Set<UserProfile>()
                .AnyAsync(p => p.UserId == request.UserId, cancellationToken);

            if (existing)
                return UserProfileResult.Failure.AlreadyExists;

            var profile = request.MapToDomain();
            profile.UserId = user.Id;

            dbContext.Set<UserProfile>().Add(profile);
            await dbContext.SaveChangesAsync(cancellationToken);

            return profile.MapToDetail<Response>();
        }
    }
}

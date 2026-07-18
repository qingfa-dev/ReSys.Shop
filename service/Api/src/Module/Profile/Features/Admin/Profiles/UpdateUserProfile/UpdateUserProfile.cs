using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.Shared.Mappings;

namespace Module.Profile.Features.Admin.Profiles.UpdateUserProfile;

public static partial class UpdateUserProfile
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is not null)
            {
                request.MapToDomain(profile);
            }
            else
            {
                profile = request.MapToDomain();
                profile.UserId = request.UserId;
                dbContext.Set<UserProfile>().Add(profile);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return profile.MapToDetail<Response>();
        }
    }
}

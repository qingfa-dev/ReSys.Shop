using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.Shared.Mappings;

namespace Module.Profile.Features.Store.Profiles.Update;

/// <summary>Updates or creates the authenticated user's profile fields.</summary>
public static partial class UpdateProfile
{
    /// <param name="UserId">The unique identifier of the user whose profile to update.</param>
    /// <param name="Request">The request containing updated profile details.</param>
    /// <param name="IsAdminBypass">When true, skips ownership check for admin-initiated operations.</param>
    public sealed record Command(Guid UserId, Request Request, bool IsAdminBypass = false) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var userId = command.UserId;

            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            if (profile is not null)
            {
                request.MapToDomain(profile);
            }
            else
            {
                profile = request.MapToDomain();
                profile.UserId = userId;
                dbContext.Set<UserProfile>().Add(profile);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return profile.MapToDetail<Response>();
        }
    }
}

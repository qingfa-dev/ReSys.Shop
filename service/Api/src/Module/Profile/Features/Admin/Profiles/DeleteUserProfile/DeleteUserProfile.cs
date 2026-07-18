using Module.Profile.Domain;

using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Profile.Features.Admin.Profiles.DeleteUserProfile;

public static partial class DeleteUserProfile
{
    public sealed record Command(Guid UserId) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
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

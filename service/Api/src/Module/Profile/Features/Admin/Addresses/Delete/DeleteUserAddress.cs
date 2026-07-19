using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;

namespace Module.Profile.Features.Admin.Addresses.Delete;

public static partial class DeleteUserAddress
{
    public sealed record Command(Guid Id, Guid UserId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.UserNotFound;

            var address = profile.Addresses.FirstOrDefault(a => a.Id == command.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            var addressType = address.AddressType;
            var wasDefault = address.IsDefault;
            var label = address.Label ?? address.Address1;

            profile.RemoveAddress(command.Id);

            if (wasDefault)
            {
                var remainingOfType = profile.Addresses
                    .Where(a => a.AddressType == addressType)
                    .ToList();

                if (remainingOfType.Count > 0)
                    remainingOfType[0].IsDefault = true;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response(command.Id, label);
        }
    }
}

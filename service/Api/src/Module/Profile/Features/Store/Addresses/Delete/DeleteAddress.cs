using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Delete;

/// <summary>Removes an address from the authenticated user's profile.</summary>
public static partial class DeleteAddress
{
    public sealed record Command(Guid UserId, Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            var address = profile.Addresses.FirstOrDefault(a => a.Id == command.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            var addressType = address.AddressType;
            var wasDefault = address.IsDefault;

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

            return new Response { Id = address.Id, Label = address.Label ?? address.Address1 };
        }
    }
}

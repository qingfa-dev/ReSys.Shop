using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Delete;

public static partial class DeleteAddress
{
    // ============ COMMAND ============
    public sealed record Command(Guid Id) : ICommand<Response>;

    // ============ COMMAND HANDLER ============
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return AddressResult.Failure.AuthRequired;

            // Resolve: Get the profile with addresses
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            // Check: Address exists in profile
            var address = profile.Addresses.FirstOrDefault(a => a.Id == command.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            var addressType = address.AddressType;
            var wasDefault = address.IsDefault;

            // Remove: Delete from profile
            profile.RemoveAddress(command.Id);

            // Business Rule: Ensure we still have a default if there are addresses left for this type
            if (wasDefault)
            {
                var remainingOfType = profile.Addresses
                    .Where(a => a.AddressType == addressType)
                    .ToList();

                if (remainingOfType.Count > 0)
                {
                    remainingOfType[0].IsDefault = true;
                }
            }

            // Await: Persist changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return deleted info
            return new Response(address.Id, address.Label ?? address.Address1);
        }
    }
}
using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Delete;

/// <summary>Removes an address from the authenticated user's profile.</summary>
public static partial class DeleteAddress
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Ensures a default address remains after deletion, then persists the change.</summary>
        /// <param name="command">The command containing the address ID to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the deleted address summary or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=address removed from profile, throws=DbUpdateException
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return AddressResult.Failure.AuthRequired;

            // Load: Get the profile with addresses
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

            // Enforce: Ensure we still have a default if there are addresses left for this type
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

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return deleted info
            // EXCEPTION: minimal delete confirmation — address ID and label only
            return new Response(address.Id, address.Label ?? address.Address1);
        }
    }
}
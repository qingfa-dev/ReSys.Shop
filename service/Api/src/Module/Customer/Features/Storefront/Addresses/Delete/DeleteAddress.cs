using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;

using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Features.Storefront.Addresses.Delete;

/// <summary>Removes an address from the authenticated user's profile.</summary>
public static partial class DeleteAddress
{
    public sealed record Command(Guid UserId, Guid Id) : ICommand<Response>;

    /// <summary>Handles the deletion of a user address.</summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Deletes a user address.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

            // Validate: Confirm profile exists
            if (profile is null)
                return UserResult.Failure.NotFound;

            // Validate: Confirm address exists on profile
            var address = profile.Addresses.FirstOrDefault(a => a.Id == command.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            var addressType = address.AddressType;
            var wasDefault = address.IsDefault;

            // Update: Remove address from the profile
            profile.RemoveAddress(command.Id);

            // Validate: Promote another address to default if needed
            if (wasDefault)
            {
                var remainingOfType = profile.Addresses
                    .Where(a => a.AddressType == addressType)
                    .ToList();

                if (remainingOfType.Count > 0)
                    remainingOfType[0].IsDefault = true;
            }

            // Call: Persist the deletion to the database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Build response from deleted address identity
            return new Response { Id = address.Id, Label = address.Label ?? address.Address1 };
        }
    }
}

using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Customer.Features.Shared.Addresses.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Features.Storefront.Addresses.Create;

/// <summary>Creates a new address for the authenticated user's profile.</summary>
public static partial class CreateAddress
{
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    /// <summary>Handles the creation of a new address for the current user.</summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Creates a new address for the current user.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Extract incoming request data
            var request = command.Request;

            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

            // Validate: Confirm profile exists before mutating addresses
            if (profile is null)
                return UserResult.Failure.NotFound;

            // Validate: Enforce maximum total addresses per profile
            if (profile.Addresses.Count >= UserProfileConstant.Constraints.MaxAddressesCount)
                return AddressResult.Failure.MaxAddressesReached;

            var sameTypeAddresses = profile.Addresses
                .Where(a => a.AddressType == request.AddressType)
                .ToList();

            // Validate: Enforce maximum addresses per type
            if (sameTypeAddresses.Count >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
                return AddressResult.Failure.MaxAddressesPerTypeReached;

            // Validate: Reject duplicate address entries
            var isDuplicate = profile.Addresses.Any(a =>
                a.Address1.Equals(request.Address1, StringComparison.OrdinalIgnoreCase) &&
                a.City.Equals(request.City, StringComparison.OrdinalIgnoreCase) &&
                a.CountryName.Equals(request.CountryName, StringComparison.OrdinalIgnoreCase) &&
                (a.Address2 ?? "").Equals(request.Address2 ?? "", StringComparison.OrdinalIgnoreCase) &&
                (a.ZipCode ?? "").Equals(request.ZipCode ?? "", StringComparison.OrdinalIgnoreCase)
            );

            if (isDuplicate)
                return AddressResult.Failure.DuplicateAddress;

            // Transform: Map validated request to new address entity
            var address = request.MapToDomain();

            // Validate: First address of its type becomes the default
            if (sameTypeAddresses.Count == 0)
                address.IsDefault = true;

            // Validate: Only one default address per type
            if (address.IsDefault)
            {
                foreach (var existing in sameTypeAddresses)
                    existing.IsDefault = false;
            }

            // Create: Attach the address to the user profile
            profile.AddAddress(address);

            // Call: Persist changes to the database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map persisted address to response DTO
            return Result<Response>.Created(address.ToResponse<Response>());
        }
    }
}

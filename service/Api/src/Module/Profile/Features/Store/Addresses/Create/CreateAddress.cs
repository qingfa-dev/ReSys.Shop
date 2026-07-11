using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Create;

/// <summary>Creates a new address for the authenticated user's profile.</summary>
public static partial class CreateAddress
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates limits and duplicates, then creates and persists a new address on the user profile.</summary>
        /// <param name="command">The command containing the address creation request.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created address response or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated && profile exists, post=address persisted to profile,
            //           throws=DbUpdateException
            var request = command.Request;

            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return AddressResult.Failure.AuthRequired;

            // Load: Get the profile for the current user including existing addresses
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            // Validate: Overall address count limit
            if (profile.Addresses.Count >= UserProfileConstant.Constraints.MaxAddressesCount)
                return AddressResult.Failure.MaxAddressesReached;

            // Validate: Per-type address count limit
            var sameTypeAddresses = profile.Addresses
                .Where(a => a.AddressType == request.AddressType)
                .ToList();

            if (sameTypeAddresses.Count >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
                return AddressResult.Failure.MaxAddressesPerTypeReached;

            // Validate: No duplicates (check relevant fields)
            var isDuplicate = profile.Addresses.Any(a =>
                a.Address1.Equals(request.Address1, StringComparison.OrdinalIgnoreCase) &&
                a.City.Equals(request.City, StringComparison.OrdinalIgnoreCase) &&
                a.CountryName.Equals(request.CountryName, StringComparison.OrdinalIgnoreCase) &&
                (a.Address2 ?? "").Equals(request.Address2 ?? "", StringComparison.OrdinalIgnoreCase) &&
                (a.ZipCode ?? "").Equals(request.ZipCode ?? "", StringComparison.OrdinalIgnoreCase)
            );

            if (isDuplicate)
                return AddressResult.Failure.DuplicateAddress;

            // Transform: Request DTO to domain entity
            var address = request.MapToDomain();

            // Enforce: 1 and only 1 default per type
            // If this is the first address of this type, force it to be default
            if (sameTypeAddresses.Count == 0)
            {
                address.IsDefault = true;
            }

            // If the new address is set as default, unset others of the same type
            if (address.IsDefault)
            {
                foreach (var existing in sameTypeAddresses)
                {
                    existing.IsDefault = false;
                }
            }

            // Add: Attach new address to the profile aggregate
            profile.AddAddress(address);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Domain entity to response DTO
            return Result<Response>.Created(address.ToResponse<Response>());
        }
    }
}
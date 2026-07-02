using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Update;

public static partial class UpdateAddress
{
    // ============ COMMAND ============
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    // ============ COMMAND HANDLER ============
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

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

            // Validate: No duplicates (check relevant fields, excluding current address)
            var isDuplicate = profile.Addresses.Any(a =>
                a.Id != command.Id &&
                a.Address1.Equals(request.Address1, StringComparison.OrdinalIgnoreCase) &&
                a.City.Equals(request.City, StringComparison.OrdinalIgnoreCase) &&
                a.CountryName.Equals(request.CountryName, StringComparison.OrdinalIgnoreCase) &&
                (a.Address2 ?? "").Equals(request.Address2 ?? "", StringComparison.OrdinalIgnoreCase) &&
                (a.ZipCode ?? "").Equals(request.ZipCode ?? "", StringComparison.OrdinalIgnoreCase)
            );

            if (isDuplicate)
                return AddressResult.Failure.DuplicateAddress;

            // Check: If address type is changing, validate per-type limit for NEW type
            if (address.AddressType != request.AddressType)
            {
                var newTypeCount = profile.Addresses.Count(a => a.AddressType == request.AddressType);
                if (newTypeCount >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
                    return AddressResult.Failure.MaxAddressesPerTypeReached;
            }

            var oldType = address.AddressType;
            var wasDefault = address.IsDefault;

            // Update: Apply changes to entity
            address.UpdateEntity(request);

            // Business Rule: 1 and only 1 default per type
            var sameTypeAddresses = profile.Addresses
                .Where(a => a.AddressType == address.AddressType && a.Id != address.Id)
                .ToList();

            // If it's the only one of its type, it must be default
            if (sameTypeAddresses.Count == 0)
            {
                address.IsDefault = true;
            }
            else if (address.IsDefault)
            {
                // If set to default, unset others of same type
                foreach (var existing in sameTypeAddresses)
                {
                    existing.IsDefault = false;
                }
            }
            else if (wasDefault && !address.IsDefault)
            {
                // If it was default and now it's not, we MUST have another default for this type
                // Pick the first one from the list and make it default
                sameTypeAddresses[0].IsDefault = true;
            }

            // If the type changed, we might need to fix the OLD type's default address
            if (oldType != address.AddressType)
            {
                var oldTypeAddresses = profile.Addresses
                    .Where(a => a.AddressType == oldType && a.Id != address.Id)
                    .ToList();

                if (oldTypeAddresses.Count > 0 && !oldTypeAddresses.Any(a => a.IsDefault))
                {
                    oldTypeAddresses[0].IsDefault = true;
                }
            }

            // Await: Persist changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Domain entity to response DTO
            return address.ToResponse<Response>();
        }
    }
}

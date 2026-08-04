using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Shared.Addresses.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Storefront.Addresses.Update;

/// <summary>Updates an existing address on the authenticated user's profile.</summary>
public static partial class UpdateAddress
{
    public sealed record Command(Guid UserId, Guid Id, Request Request) : ICommand<Response>;

    /// <summary>Handles the update of an existing address.</summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates an existing address.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Extract incoming request data
            var request = command.Request;

            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

            // Validate: Confirm profile and address exist
            if (profile is null)
                return UserResult.Failure.NotFound;

            var address = profile.Addresses.FirstOrDefault(a => a.Id == command.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            // Validate: Reject duplicate address entries
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

            // Validate: Enforce per-type address limit when type changes
            if (address.AddressType != request.AddressType)
            {
                var newTypeCount = profile.Addresses.Count(a => a.AddressType == request.AddressType);
                if (newTypeCount >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
                    return AddressResult.Failure.MaxAddressesPerTypeReached;
            }

            var oldType = address.AddressType;
            var wasDefault = address.IsDefault;

            // Transform: Apply request fields to existing address entity
            address.UpdateEntity(request);

            // Validate: Manage default address propagation across types
            var sameTypeAddresses = profile.Addresses
                .Where(a => a.AddressType == address.AddressType && a.Id != address.Id)
                .ToList();

            if (sameTypeAddresses.Count == 0)
            {
                address.IsDefault = true;
            }
            else if (address.IsDefault)
            {
                foreach (var existing in sameTypeAddresses)
                    existing.IsDefault = false;
            }
            else if (wasDefault && !address.IsDefault)
            {
                sameTypeAddresses[0].IsDefault = true;
            }

            if (oldType != address.AddressType)
            {
                var oldTypeAddresses = profile.Addresses
                    .Where(a => a.AddressType == oldType && a.Id != address.Id)
                    .ToList();

                if (oldTypeAddresses.Count > 0 && !oldTypeAddresses.Any(a => a.IsDefault))
                    oldTypeAddresses[0].IsDefault = true;
            }

            // Call: Persist changes to the database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map updated address to response DTO
            return address.ToResponse<Response>();
        }
    }
}

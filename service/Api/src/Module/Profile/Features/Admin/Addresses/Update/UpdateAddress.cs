using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Mappings;

namespace Module.Profile.Features.Admin.Addresses.Update;

public static partial class UpdateAddress
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var profile = await dbContext.Set<UserProfile>()
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.UserNotFound;

            var address = profile.Addresses.FirstOrDefault(a => a.Id == command.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            var isDuplicate = profile.Addresses.Any(a =>
                a.Id != command.Id &&
                a.Address1.Equals(request.Address1, StringComparison.OrdinalIgnoreCase) &&
                a.City.Equals(request.City, StringComparison.OrdinalIgnoreCase) &&
                a.CountryName.Equals(request.CountryName, StringComparison.OrdinalIgnoreCase) &&
                (a.Address2 ?? "").Equals(request.Address2 ?? "", StringComparison.OrdinalIgnoreCase) &&
                (a.ZipCode ?? "").Equals(request.ZipCode ?? "", StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
                return AddressResult.Failure.DuplicateAddress;

            if (address.AddressType != request.AddressType)
            {
                var newTypeCount = profile.Addresses.Count(a => a.AddressType == request.AddressType);
                if (newTypeCount >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
                    return AddressResult.Failure.MaxAddressesPerTypeReached;
            }

            var oldType = address.AddressType;
            var wasDefault = address.IsDefault;

            address.UpdateEntity(request);

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

            await dbContext.SaveChangesAsync(cancellationToken);

            return address.ToResponse<Response>();
        }
    }
}

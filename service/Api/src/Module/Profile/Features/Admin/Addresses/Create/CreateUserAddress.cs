using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Mappings;

namespace Module.Profile.Features.Admin.Addresses.Create;

public static partial class CreateUserAddress
{
    public sealed record Command(Request Request) : ICommand<Response>;

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

            if (profile.Addresses.Count >= UserProfileConstant.Constraints.MaxAddressesCount)
                return AddressResult.Failure.MaxAddressesReached;

            var sameTypeAddresses = profile.Addresses
                .Where(a => a.AddressType == request.AddressType)
                .ToList();

            if (sameTypeAddresses.Count >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
                return AddressResult.Failure.MaxAddressesPerTypeReached;

            var isDuplicate = profile.Addresses.Any(a =>
                a.Address1.Equals(request.Address1, StringComparison.OrdinalIgnoreCase) &&
                a.City.Equals(request.City, StringComparison.OrdinalIgnoreCase) &&
                a.CountryName.Equals(request.CountryName, StringComparison.OrdinalIgnoreCase) &&
                (a.Address2 ?? "").Equals(request.Address2 ?? "", StringComparison.OrdinalIgnoreCase) &&
                (a.ZipCode ?? "").Equals(request.ZipCode ?? "", StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
                return AddressResult.Failure.DuplicateAddress;

            var address = request.MapToDomain();

            if (sameTypeAddresses.Count == 0)
                address.IsDefault = true;

            if (address.IsDefault)
            {
                foreach (var existing in sameTypeAddresses)
                    existing.IsDefault = false;
            }

            profile.AddAddress(address);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(address.ToResponse<Response>());
        }
    }
}

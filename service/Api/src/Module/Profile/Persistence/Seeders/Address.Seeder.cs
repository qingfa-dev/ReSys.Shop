using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;

namespace Module.Profile.Persistence.Seeders;

public sealed class AddressSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 60;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasAddresses = await HasDataAsync<Address>(cancellationToken);
        if (hasAddresses)
        {
            return Result.Ok();
        }

        var us = await Context.Set<Country>().FirstOrDefaultAsync(c => c.IsoCode == "US", cancellationToken);
        if (us is null)
            return Result.Ok();
        var states = await Context.Set<State>().Where(s => s.CountryId == us.Id).ToListAsync(cancellationToken);
        var ny = states.First(s => s.Abbreviation == "NY");
        var ca = states.First(s => s.Abbreviation == "CA");
        var il = states.First(s => s.Abbreviation == "IL");

        var profiles = await Context.Set<UserProfile>().ToListAsync(cancellationToken);

        foreach (var profile in profiles)
        {
            (string address1, string city, string zipCode, string label, State state) = profile.FirstName switch
            {
                "Admin" => ("123 Main Street", "New York", "10001", "Office", ny),
                "Manager" => ("456 Oak Avenue", "Los Angeles", "90001", "Office", ca),
                _ => ("789 Pine Road", "Chicago", "60601", "Home", il),
            };

            Result<Address> addressResult = AddressMethod.Create(
                firstName: profile.FirstName,
                address1: address1,
                city: city,
                countryName: us.Name,
                addressType: AddressType.Shipping,
                lastName: profile.LastName,
                zipCode: zipCode,
                isDefault: true,
                label: label,
                stateProvince: state.Name,
                countryCode: us.IsoCode,
                stateCode: state.Abbreviation,
                userProfileId: profile.Id);

            if (addressResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to create address for profile '{profile.Id}': {addressResult.Errors.FirstOrDefault().Message}");
            }

            Context.Set<Address>().Add(addressResult.Value);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
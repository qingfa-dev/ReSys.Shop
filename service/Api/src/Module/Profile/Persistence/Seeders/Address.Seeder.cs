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

            Result<Address> shippingResult = AddressMethod.Create(
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

            if (shippingResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to create shipping address for profile '{profile.Id}': {shippingResult.Errors.FirstOrDefault().Message}");
            }

            Context.Set<Address>().Add(shippingResult.Value);

            (string billingAddress1, string billingCity, string billingZip) = profile.FirstName switch
            {
                "Admin" => ("321 Broadway", "New York", "10002"),
                "Manager" => ("654 Elm Boulevard", "Los Angeles", "90002"),
                _ => ("987 Oak Lane", "Chicago", "60602"),
            };

            Result<Address> billingResult = AddressMethod.Create(
                firstName: profile.FirstName,
                address1: billingAddress1,
                city: billingCity,
                countryName: us.Name,
                addressType: AddressType.Billing,
                lastName: profile.LastName,
                zipCode: billingZip,
                isDefault: false,
                label: "Billing",
                stateProvince: state.Name,
                countryCode: us.IsoCode,
                stateCode: state.Abbreviation,
                userProfileId: profile.Id);

            if (billingResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to create billing address for profile '{profile.Id}': {billingResult.Errors.FirstOrDefault().Message}");
            }

            Context.Set<Address>().Add(billingResult.Value);
        }

        await SaveChangesWithIdempotencyAsync(cancellationToken);

        return Result.Ok();
    }
}
using Module.Location.Domain.Countries;

namespace Module.Location.Persistence.Seeders;

/// <summary>Seeds default country data into the database.</summary>
public sealed class CountrySeeder(IApplicationDbContext context) : AbstractDataSeeder(context: context)
{
    /// <summary>Execution order for the seeder pipeline.</summary>
    public override int Order => 10;

    /// <summary>Executes the seeding process for default countries.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure of the seeding operation.</returns>
    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasCountries = await HasDataAsync<Country>(cancellationToken: cancellationToken);
        if (hasCountries)
        {
            return Result.Ok();
        }

        var us = CreateCountry(name: "United States", isoCode: "US", callingCode: "+1", statesRequired: true);
        var vietnam = CreateCountry(name: "Vietnam", isoCode: "VN", callingCode: "+84", statesRequired: true);

        Context.Set<Country>().AddRange(entities: [us, vietnam]);

        await Context.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Ok();
    }

    private static Country CreateCountry(string name, string isoCode, string callingCode, bool statesRequired)
    {
        var utcNow = DateTimeOffset.UtcNow;
        return new Country
        {
            Name = name,
            IsoCode = isoCode,
            CallingCode = callingCode,
            StatesRequired = statesRequired,
            IsActive = true,
            CreatedAtUtc = utcNow,
            CreatedBy = "System"
        };
    }
}
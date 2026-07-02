using Module.Location.Domain.Countries;
using Module.Location.Domain.States;

using Shared.Operational.Persistence.Data;

using Microsoft.EntityFrameworkCore;

namespace Module.Location.Persistence.Seeders;

/// <summary>Seeds default state data into the database.</summary>
public sealed class StateSeeder(IApplicationDbContext context) : AbstractDataSeeder(context: context)
{
    /// <summary>Execution order for the seeder pipeline.</summary>
    public override int Order => 20;

    /// <summary>Executes the seeding process for default states.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure of the seeding operation.</returns>
    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStates = await HasDataAsync<State>(cancellationToken: cancellationToken);
        if (hasStates)
        {
            return Result.Ok();
        }

        var us = await Context.Set<Country>()
            .FirstOrDefaultAsync(predicate: c => c.IsoCode == "US", cancellationToken: cancellationToken);
        var vietnam = await Context.Set<Country>()
            .FirstOrDefaultAsync(predicate: c => c.IsoCode == "VN", cancellationToken: cancellationToken);

        if (us is null || vietnam is null)
        {
            return Result.Ok();
        }

        var usStates = CreateUsStates(countryId: us.Id);
        var vnProvinces = CreateVietnamProvinces(countryId: vietnam.Id);

        Context.Set<State>().AddRange(entities: [.. usStates, .. vnProvinces]);

        await Context.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Ok();
    }

    private static IEnumerable<State> CreateUsStates(Guid countryId)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var states = new (string Name, string Abbreviation)[]
        {
            ("Alabama", "AL"), ("Alaska", "AK"), ("Arizona", "AZ"), ("Arkansas", "AR"),
            ("California", "CA"), ("Colorado", "CO"), ("Connecticut", "CT"), ("Delaware", "DE"),
            ("Florida", "FL"), ("Georgia", "GA"), ("Hawaii", "HI"), ("Idaho", "ID"),
            ("Illinois", "IL"), ("Indiana", "IN"), ("Iowa", "IA"), ("Kansas", "KS"),
            ("Kentucky", "KY"), ("Louisiana", "LA"), ("Maine", "ME"), ("Maryland", "MD"),
            ("Massachusetts", "MA"), ("Michigan", "MI"), ("Minnesota", "MN"), ("Mississippi", "MS"),
            ("Missouri", "MO"), ("Montana", "MT"), ("Nebraska", "NE"), ("Nevada", "NV"),
            ("New Hampshire", "NH"), ("New Jersey", "NJ"), ("New Mexico", "NM"), ("New York", "NY"),
            ("North Carolina", "NC"), ("North Dakota", "ND"), ("Ohio", "OH"), ("Oklahoma", "OK"),
            ("Oregon", "OR"), ("Pennsylvania", "PA"), ("Rhode Island", "RI"), ("South Carolina", "SC"),
            ("South Dakota", "SD"), ("Tennessee", "TN"), ("Texas", "TX"), ("Utah", "UT"),
            ("Vermont", "VT"), ("Virginia", "VA"), ("Washington", "WA"), ("West Virginia", "WV"),
            ("Wisconsin", "WI"), ("Wyoming", "WY")
        };

        return states.Select(selector: s => new State
        {
            Name = s.Name,
            Abbreviation = s.Abbreviation,
            CountryId = countryId,
            CreatedAtUtc = utcNow,
            CreatedBy = "System"
        });
    }

    private static IEnumerable<State> CreateVietnamProvinces(Guid countryId)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var provinces = new (string Name, string Abbreviation)[]
        {
            ("An Giang", "AG"), ("Bac Giang", "BG"), ("Bac Kan", "BK"), ("Bac Lieu", "BL"),
            ("Bac Ninh", "BN"), ("Ben Tre", "BT"), ("Binh Dinh", "BD"), ("Binh Duong", "BU"),
            ("Binh Phuoc", "BP"), ("Binh Thuan", "BTN"), ("Ca Mau", "CM"), ("Cao Bang", "CB"),
            ("Dak Lak", "DL"), ("Dak Nong", "DN"), ("Dien Bien", "DB"), ("Dong Nai", "DNI"),
            ("Dong Thap", "DTH"), ("Gia Lai", "GL"), ("Ha Giang", "HGI"), ("Ha Nam", "HM"),
            ("Ha Tinh", "HT"), ("Hai Duong", "HD"), ("Hau Giang", "HG"), ("Hoa Binh", "HB"),
            ("Hung Yen", "HY"), ("Khanh Hoa", "KH"), ("Kien Giang", "KG"), ("Kon Tum", "KT"),
            ("Lai Chau", "LC"), ("Lam Dong", "LD"), ("Lang Son", "LS"), ("Lao Cai", "LCA"),
            ("Long An", "LNA"), ("Nam Dinh", "ND"), ("Nghe An", "NA"), ("Ninh Binh", "NB"),
            ("Ninh Thuan", "NT"), ("Phu Tho", "PT"), ("Phu Yen", "PY"), ("Quang Binh", "QB"),
            ("Quang Nam", "QNM"), ("Quang Ngai", "QNG"), ("Quang Ninh", "QNI"), ("Quang Tri", "QT"),
            ("Soc Trang", "ST"), ("Son La", "SL"), ("Tay Ninh", "TN"), ("Thai Binh", "THB"),
            ("Thai Nguyen", "TNG"), ("Thanh Hoa", "THC"), ("Thua Thien Hue", "TTH"), ("Tien Giang", "TGI"),
            ("Tra Vinh", "TV"), ("Tuyen Quang", "TQ"), ("Vinh Long", "VL"), ("Vinh Phuc", "VPC"),
            ("Yen Bai", "YB")
        };

        return provinces.Select(selector: p => new State
        {
            Name = p.Name,
            Abbreviation = p.Abbreviation,
            CountryId = countryId,
            CreatedAtUtc = utcNow,
            CreatedBy = "System"
        });
    }
}

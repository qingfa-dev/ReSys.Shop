using Module.Location.Domain.Countries;

using Shared.Application.Mediators.Queries;

namespace Module.Location.Features.Shared.Queries;

public sealed record CountryExistsByIsoQuery(string IsoCode) : IQuery<bool>;

/// <summary>Checks whether a country exists for a given ISO code (case-insensitive).</summary>
public sealed class CountryExistsByIsoQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<CountryExistsByIsoQuery, bool>
{
    public async Task<Result<bool>> Handle(CountryExistsByIsoQuery query, CancellationToken ct)
    {
        var isoCode = query.IsoCode.ToUpperInvariant();
        var exists = await dbContext.Set<Country>()
            .AnyAsync(c => c.IsoCode.ToUpper() == isoCode, ct);
        return Result<bool>.Ok(exists);
    }
}

using Module.Location.Domain.States;

using Shared.Application.Contracts.Location;

namespace Module.Location.Features.Shared.Queries;

/// <summary>Checks whether a state exists for a given country code and state code (case-insensitive).</summary>
public sealed class StateExistsByIsoQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<StateExistsByIsoQuery, bool>
{
    public async Task<Result<bool>> Handle(StateExistsByIsoQuery query, CancellationToken ct)
    {
        var countryCode = query.CountryCode.ToUpperInvariant();
        var stateCode = query.StateCode.ToUpperInvariant();
        var exists = await dbContext.Set<State>()
            .AnyAsync(s => s.Abbreviation.ToUpper() == stateCode
                        && s.Country.IsoCode.ToUpper() == countryCode, ct);
        return Result<bool>.Ok(exists);
    }
}

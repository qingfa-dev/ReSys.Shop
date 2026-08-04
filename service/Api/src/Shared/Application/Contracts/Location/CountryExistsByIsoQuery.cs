using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Location;

public sealed record CountryExistsByIsoQuery(string IsoCode) : IQuery<bool>;

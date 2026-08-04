using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Location;

public sealed record StateExistsByIsoQuery(string CountryCode, string StateCode) : IQuery<bool>;

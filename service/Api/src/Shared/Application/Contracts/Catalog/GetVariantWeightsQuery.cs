using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Catalog;

public sealed record GetVariantWeightsQuery(IReadOnlyList<Guid> VariantIds) : IQuery<IReadOnlyDictionary<Guid, decimal>>;

using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Catalog;

public sealed record GetVariantDiscontinuedStatusesQuery : IQuery<IReadOnlyDictionary<Guid, bool>>
{
    public IReadOnlyList<Guid> VariantIds { get; init; } = [];
}

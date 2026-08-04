using Module.Catalog.Domain.Products.Variants;
using Shared.Application.Contracts.Catalog;

namespace Module.Catalog.Features.Storefront.Contracts.GetVariantDiscontinuedStatuses;

public sealed class GetVariantDiscontinuedStatusesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetVariantDiscontinuedStatusesQuery, IReadOnlyDictionary<Guid, bool>>
{
    public async Task<Result<IReadOnlyDictionary<Guid, bool>>> Handle(
        GetVariantDiscontinuedStatusesQuery query, CancellationToken cancellationToken)
    {
        var ids = query.VariantIds;

        if (ids.Count == 0)
            return Result<IReadOnlyDictionary<Guid, bool>>.Ok(
                new Dictionary<Guid, bool>());

        var discontinued = await dbContext.Set<Variant>()
            .Where(v => ids.Contains(v.Id) && v.DiscontinuedOn != null)
            .Select(v => v.Id)
            .ToHashSetAsync(cancellationToken);

        IReadOnlyDictionary<Guid, bool> result = ids
            .ToDictionary(id => id, id => discontinued.Contains(id));

        return Result<IReadOnlyDictionary<Guid, bool>>.Ok(result);
    }
}

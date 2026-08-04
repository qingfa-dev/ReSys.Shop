using Module.Catalog.Domain.Products.Variants;

using Shared.Application.Contracts.Catalog;

namespace Module.Catalog.Features.Storefront.Contracts.GetVariantWeights;

public sealed class GetVariantWeightsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetVariantWeightsQuery, IReadOnlyDictionary<Guid, decimal>>
{
    public async Task<Result<IReadOnlyDictionary<Guid, decimal>>> Handle(
        GetVariantWeightsQuery query, CancellationToken cancellationToken)
    {
        var ids = query.VariantIds;

        if (ids.Count == 0)
            return Result<IReadOnlyDictionary<Guid, decimal>>.Ok(
                new Dictionary<Guid, decimal>());

        var weights = await dbContext.Set<Variant>()
            .Where(v => ids.Contains(v.Id))
            .Select(v => new { v.Id, v.Weight })
            .ToDictionaryAsync(v => v.Id, v => v.Weight ?? 0m, cancellationToken);

        IReadOnlyDictionary<Guid, decimal> result = weights;
        return Result<IReadOnlyDictionary<Guid, decimal>>.Ok(result);
    }
}

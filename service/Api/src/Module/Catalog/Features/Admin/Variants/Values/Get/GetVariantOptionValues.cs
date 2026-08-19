using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Options;

namespace Module.Catalog.Features.Admin.Variants.Values.Get;

/// <summary>
/// Defines the use case for retrieving variant option values with assigned state.
/// </summary>
public static partial class GetVariantOptionValues
{
    public sealed record Query(Guid VariantId, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=request.VariantId!=Guid.Empty, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Variant must exist before querying assigned option values
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == request.VariantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(request.VariantId);

            // Load: All option values with their option type for the full selection list
            var allOptionValues = await dbContext.Set<OptionValue>()
                .AsNoTracking()
                .Include(x => x.OptionType)
                .ToListAsync(cancellationToken);

            // Load: Already-assigned option value IDs for this variant
            var assignedOptionValueIds = await dbContext.Set<OptionValueVariant>()
                .Where(x => x.VariantId == request.VariantId)
                .Select(x => x.OptionValueId)
                .ToHashSetAsync(cancellationToken);

            // Transform: Enrich each option value with its assignment status
            var items = allOptionValues.Select(ov => new Response
            {
                OptionValueId = ov.Id,
                OptionTypeId = ov.OptionTypeId,
                OptionTypeName = ov.OptionType.Name,
                Name = ov.Name,
                Presentation = ov.Presentation,
                IsAssigned = assignedOptionValueIds.Contains(ov.Id)
            }).OrderBy(i => i.OptionTypeName).ThenBy(i => i.Name).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
        }
    }
}
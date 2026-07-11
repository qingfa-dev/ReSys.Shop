using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

public static partial class GetVariantOptionValues
{
    public sealed record Query(Guid VariantId) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves all option values with their assigned state for a variant.
        /// </summary>
        /// <param name="request">The query containing the variant ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the variant option values and their assignment status.</returns>
        // Contract: pre=request.VariantId!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == request.VariantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(request.VariantId);

            var allOptionValues = await dbContext.Set<OptionValue>()
                .AsNoTracking()
                .Include(x => x.OptionType)
                .ToListAsync(cancellationToken);

            var assignedOptionValueIds = await dbContext.Set<OptionValueVariant>()
                .Where(x => x.VariantId == request.VariantId)
                .Select(x => x.OptionValueId)
                .ToHashSetAsync(cancellationToken);

            var items = allOptionValues.Select(ov => new Response.OptionValueItem
            {
                OptionValueId = ov.Id,
                OptionTypeId = ov.OptionTypeId,
                OptionTypeName = ov.OptionType.Name,
                Name = ov.Name,
                Presentation = ov.Presentation,
                IsAssigned = assignedOptionValueIds.Contains(ov.Id)
            }).ToList();

            return new Response { Items = items };
        }
    }
}

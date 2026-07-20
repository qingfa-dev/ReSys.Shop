using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.GetById;

public static partial class GetVariantById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>Handler for getting a variant by ID.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets a product variant by ID.</summary>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch variant with prices, option values, and images
            var entity = await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .Include(x => x.OptionValueVariants)
                    .ThenInclude(ovv => ovv.OptionValue)
                .Include(x => x.VariantImages)
                .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);

            // Check: Return not-found if no variant matches the requested ID
            if (entity is null)
                return VariantResult.Errors.NotFound(query.Id);

            // Transform: Map domain entity to response DTO
            return Result<Response>.Ok(
                entity.MapToDetail<Response>());
        }
    }
}
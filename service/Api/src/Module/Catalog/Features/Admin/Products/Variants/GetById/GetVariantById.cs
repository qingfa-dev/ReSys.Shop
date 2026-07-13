using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.GetById;

public static partial class GetVariantById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .Include(x => x.OptionValueVariants)
                    .ThenInclude(ovv => ovv.OptionValue)
                .Include(x => x.VariantImages)
                .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);

            if (entity is null)
                return VariantResult.Errors.NotFound(query.Id);

            return Result<Response>.Ok(
                entity.MapToDetail<Response>());
        }
    }
}
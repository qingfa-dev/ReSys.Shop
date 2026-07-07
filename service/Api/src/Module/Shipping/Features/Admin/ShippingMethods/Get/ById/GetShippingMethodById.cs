using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.ById;

public static partial class GetShippingMethodById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var method = await dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            return method.MapToDetail<Response>();
        }
    }
}

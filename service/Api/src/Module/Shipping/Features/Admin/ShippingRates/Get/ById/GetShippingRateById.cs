using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Get.ById;

public static partial class GetShippingRateById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var rate = await dbContext.Set<ShippingRate>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

            if (rate is null)
                return ShippingRateResult.Errors.NotFound(query.Id);

            return rate.MapToDetail<Response>();
        }
    }
}

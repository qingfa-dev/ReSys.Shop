using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Shared.Mappings;

namespace Module.Promotions.Features.Admin.CouponCodes.Get.ById;
/// <summary>Gets a coupon code by its ID.</summary>
public static partial class GetCouponCodeById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        /// <summary>Handles retrieving a coupon code by ID.</summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The coupon code response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Query: Get coupon code by ID.
            var couponCode = await dbContext.Set<CouponCode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

            // Check: Verify the coupon code exists.
            if (couponCode is null)
                return CouponCodeResult.Errors.NotFound(query.Id);

            // Map: Return coupon code details.
            return couponCode.MapToDetail<Response>();
        }
    }
}

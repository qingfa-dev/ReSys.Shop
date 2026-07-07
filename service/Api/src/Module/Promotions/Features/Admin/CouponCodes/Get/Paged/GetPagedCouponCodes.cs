using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Shared.Mappings;

namespace Module.Promotions.Features.Admin.CouponCodes.Get.Paged;
/// <summary>Gets a paged list of coupon codes.</summary>
public static partial class GetPagedCouponCodes
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing paged coupon codes.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of coupon codes.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Query: Retrieve all coupon codes with querying options.
            var pagedResult = await dbContext.Set<CouponCode>()
                .AsNoTracking()
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(c => c.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

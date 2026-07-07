using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionCategories.Get.Paged;
/// <summary>Gets a paged list of promotion categories.</summary>
public static partial class GetPagedPromotionCategories
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing paged promotion categories.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of promotion categories.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Query: Retrieve all promotion categories with querying options.
            var pagedResult = await dbContext.Set<PromotionCategory>()
                .AsNoTracking()
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(c => c.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

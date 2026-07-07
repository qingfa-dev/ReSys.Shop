using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.Promotions.Get.Paged;
/// <summary>Gets a paged list of promotions.</summary>
public static partial class GetPagedPromotions
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing paged promotions.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of promotions.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Query: Retrieve promotions ordered by creation date with querying options.
            var pagedResult = await dbContext.Set<Promotion>()
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAtUtc)
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(x => x.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

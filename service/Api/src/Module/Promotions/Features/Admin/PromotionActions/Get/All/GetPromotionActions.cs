using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;

using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Features.Admin.PromotionActions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionActions.Get.All;

public static partial class GetPromotionActions
{
    public sealed record Query(Guid PromotionId, QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var pagedResult = await dbContext.Set<PromotionAction>()
                .AsNoTracking()
                .Where(a => a.PromotionId == request.PromotionId)
                .OrderBy(a => a.CreatedAtUtc)
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(a => a.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;

using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionRules.Get.All;

public static partial class GetPromotionRules
{
    public sealed record Query(Guid PromotionId, QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var pagedResult = await dbContext.Set<PromotionRule>()
                .AsNoTracking()
                .Where(r => r.PromotionId == request.PromotionId)
                .OrderBy(r => r.CreatedAtUtc)
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(r => r.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

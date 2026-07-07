using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Features.Storefront.Promotions;
/// <summary>Lists active promotions for the storefront.</summary>
public static partial class ListActivePromotions
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing active promotions.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of active promotions.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;
            var now = DateTimeOffset.UtcNow;

            // Query: Retrieve active, non-expired promotions with querying options.
            var pagedResult = await dbContext.Set<Promotion>().AsNoTracking()
                .Where(p => p.Active && !p.IsDeleted
                    && (!p.StartsAtUtc.HasValue || p.StartsAtUtc <= now)
                    && (!p.ExpiresAtUtc.HasValue || p.ExpiresAtUtc >= now))
                .OrderBy(p => p.Position)
                .ApplyQueryOptions(parameters)
                .Select(p => new Response
                {
                    Id = p.Id, Name = p.Name, Description = p.Description,
                    Kind = p.Kind.ToString(), Path = p.Path, ExpiresAtUtc = p.ExpiresAtUtc
                })
                .ToPagedOrAllAsync(x => x, parameters, cancellationToken);

            return pagedResult;
        }
    }
}

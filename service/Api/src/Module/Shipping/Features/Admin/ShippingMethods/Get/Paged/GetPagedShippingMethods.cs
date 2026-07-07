using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.Paged;
/// <summary>Gets a paged list of shipping methods.</summary>
public static partial class GetPagedShippingMethods
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing paged shipping methods.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of shipping methods.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            _ = logger;
            var parameters = request.Parameters;

            // Query: Retrieve all shipping methods ordered by position with querying options.
            var pagedResult = await dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .OrderBy(m => m.Position).ThenBy(m => m.Name)
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

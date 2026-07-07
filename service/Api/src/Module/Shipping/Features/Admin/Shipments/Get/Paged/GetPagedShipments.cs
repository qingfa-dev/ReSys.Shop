using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Mappings;

namespace Module.Shipping.Features.Admin.Shipments.Get.Paged;
/// <summary>Gets a paged list of shipments.</summary>
public static partial class GetPagedShipments
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing paged shipments.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of shipments.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            _ = logger;
            var parameters = request.Parameters;

            // Query: Retrieve all shipments ordered by creation date with querying options.
            var pagedResult = await dbContext.Set<Shipment>().AsNoTracking()
                .OrderByDescending(s => s.CreatedAtUtc)
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(s => s.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

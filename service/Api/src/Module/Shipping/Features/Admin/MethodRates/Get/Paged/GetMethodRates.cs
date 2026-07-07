using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.MethodRates.Get.Paged;
/// <summary>Gets a paged list of rates for a shipping method.</summary>
public static partial class GetMethodRates
{
    public sealed record Query(Guid MethodId, QueryingParameters Parameters) : IPagedQuery<Response>;
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing method rates.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of shipping rates.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            _ = logger;
            var parameters = request.Parameters;

            // Query: Retrieve rates for the given shipping method with querying options.
            var query = dbContext.Set<ShippingRate>().AsNoTracking()
                .Where(r => r.ShippingMethodId == request.MethodId)
                .OrderBy(r => r.Cost)
                .ApplyQueryOptions(parameters);

            var pagedResult = await query
                .Select(r => new Response
                {
                    Id = r.Id, Name = r.Name, Cost = r.Cost, FinalPrice = r.FinalPrice,
                    ShippingMethodId = r.ShippingMethodId, Selected = r.Selected,
                    DeliveryRange = r.DeliveryRange, CreatedAtUtc = r.CreatedAtUtc
                })
                .ToPagedOrAllAsync(x => x, parameters, cancellationToken);

            return pagedResult;
        }
    }
}

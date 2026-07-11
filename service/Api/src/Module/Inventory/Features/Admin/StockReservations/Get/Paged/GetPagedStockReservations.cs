using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockReservations.Get.Paged;

/// <summary>Returns a paginated list of stock reservations with querying and filtering support.</summary>
public static partial class GetPagedStockReservations
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Applies filtering, sorting, and pagination from parameters and returns paged reservations.</summary>
        /// <param name="request">The query containing paging and filter parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of stock reservations.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            // Parse: Validate and parse querying parameters for pagination, filtering, and sorting
            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Load: Retrieve stock reservations, apply querying options, and map to paged result.
            var pagedResult = await dbContext.Set<StockReservation>()
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToListItem<Response>(), cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}

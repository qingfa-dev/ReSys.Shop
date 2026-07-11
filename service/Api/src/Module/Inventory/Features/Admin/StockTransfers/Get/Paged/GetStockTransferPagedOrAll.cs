using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Get.Paged;

/// <summary>Returns a paginated or full list of stock transfers with basic transfer info.</summary>
public static partial class GetStockTransferPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Parses parameters and returns paged transfer results with location and item count info.</summary>
        /// <param name="request">The query containing paging and filter parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of stock transfers.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;
            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            var pagedResult = await dbContext.Set<StockTransfer>()
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(model: parseAll.Value,
                projection: x => new Response
                {
                    Id = x.Id,
                    Number = x.Number,
                    Reference = x.Reference,
                    State = x.State.ToString(),
                    SourceLocationId = x.SourceLocationId,
                    DestinationLocationId = x.DestinationLocationId,
                    TotalItems = x.TransferItems.Count,
                    CreatedAtUtc = x.CreatedAtUtc
                }, ct: cancellationToken);

            return pagedResult;
        }
    }
}

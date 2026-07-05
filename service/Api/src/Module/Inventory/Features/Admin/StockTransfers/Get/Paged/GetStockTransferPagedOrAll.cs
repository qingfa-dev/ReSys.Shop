using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Get.Paged;

public static partial class GetStockTransferPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Contract: pre=request!=null, post=result!=null
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

using Module.Inventory.Services;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Admin.StockItems.Summary;

/// <summary>Handles retrieval of consolidated per-variant stock summary across all locations.</summary>
public static partial class GetStockSummary
{
    public sealed record Query : IQuery<List<Response>>;

    public sealed class QueryHandler(IStockChecker stockChecker)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Executes the get stock summary query.</summary>
        /// <param name="request">The query (no parameters needed).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of per-variant stock summaries.</returns>
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve consolidated stock summary via StockChecker
            var summary = await stockChecker.GetStockSummaryAsync(cancellationToken);
            return summary.Select(x => new Response(x)).ToList();
        }
    }
}

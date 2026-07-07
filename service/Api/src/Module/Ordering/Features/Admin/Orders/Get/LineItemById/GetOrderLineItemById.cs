using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItemById;

/// <summary>Gets a single line item for an order.</summary>
public static partial class GetOrderLineItemById
{
    public class Response
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public decimal Total { get; init; }
        public decimal AdjustmentTotal { get; init; }
        public decimal TaxTotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; init; }
    }

    public sealed record Query(Guid OrderId, Guid LineItemId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var lineItem = await dbContext.Set<LineItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    li => li.Id == query.LineItemId && li.OrderId == query.OrderId,
                    cancellationToken);

            if (lineItem is null)
                return LineItemResult.Errors.NotFound(query.LineItemId);

            return new Response
            {
                Id = lineItem.Id,
                VariantId = lineItem.VariantId,
                Quantity = lineItem.Quantity,
                Price = lineItem.Price,
                Total = lineItem.Total,
                AdjustmentTotal = lineItem.AdjustmentTotal,
                TaxTotal = lineItem.TaxTotal,
                Currency = lineItem.Currency,
                CreatedAtUtc = lineItem.CreatedAtUtc
            };
        }
    }
}

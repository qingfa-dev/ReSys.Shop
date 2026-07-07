using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Features.Admin.Orders.Get.AdjustmentById;

/// <summary>Gets a single adjustment for an order.</summary>
public static partial class GetOrderAdjustmentById
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Label { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public bool Eligible { get; init; }
        public bool Included { get; init; }
        public bool Mandatory { get; init; }
        public string State { get; init; } = string.Empty;
        public Guid SourceId { get; init; }
        public string SourceType { get; init; } = string.Empty;
        public Guid OrderId { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }

    public sealed record Query(Guid OrderId, Guid AdjustmentId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var adjustment = await dbContext.Set<Adjustment>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a => a.Id == query.AdjustmentId && a.OrderId == query.OrderId,
                    cancellationToken);

            if (adjustment is null)
                return AdjustmentResult.Errors.NotFound(query.AdjustmentId);

            return new Response
            {
                Id = adjustment.Id,
                Label = adjustment.Label,
                Amount = adjustment.Amount,
                Eligible = adjustment.Eligible,
                Included = adjustment.Included,
                Mandatory = adjustment.Mandatory,
                State = adjustment.State,
                SourceId = adjustment.SourceId,
                SourceType = adjustment.SourceType,
                OrderId = adjustment.OrderId,
                CreatedAtUtc = adjustment.CreatedAtUtc
            };
        }
    }
}

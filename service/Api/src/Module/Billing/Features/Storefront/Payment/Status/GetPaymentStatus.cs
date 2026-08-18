using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.Shared.Mappings;


namespace Module.Billing.Features.Storefront.Payment.Status;

/// <summary>Retrieves the payment state of the latest capture for the caller's order.</summary>
public static partial class GetPaymentStatus
{
    public sealed record Query(Guid OrderId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Returns the payment state of the latest capture for the caller's order.</summary>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Check: user must be authenticated and own the order.
            var userId = Guid.TryParse(currentUser.UserId, out var parsed) ? parsed : (Guid?)null;
            if (userId is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Order ownership gate.
            var order = await dbContext.Set<Ordering.Domain.Orders.Order>()
                .FirstOrDefaultAsync(o => o.Id == query.OrderId && o.UserId == userId, cancellationToken);
            if (order is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Latest payment capture for the order.
            var payment = await dbContext.Set<PaymentCapture>()
                .Where(p => p.OrderId == query.OrderId)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            var response = payment.MapToStoreDetail<Response>();
            return response with { IsCompleted = payment.State == PaymentRecordState.Completed };
        }
    }
}

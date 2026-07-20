using Module.Payment.Features.Admin.Payments.Shared.Mappings;

using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Get.ById;

/// <summary>Retrieves a payment by its identifier.</summary>
public static partial class GetPaymentById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Retrieves a payment by its identifier.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Payment capture by ID — no-tracking for read-only
            var payment = await dbContext.Set<PaymentCapture>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            // Check: Payment must exist
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Map: Payment → response DTO
            return payment.MapToDetail<Response>();
        }
    }
}

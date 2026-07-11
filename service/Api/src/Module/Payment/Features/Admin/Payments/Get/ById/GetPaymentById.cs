using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Get.ById;

/// <summary>Retrieves a payment record by its unique identifier with full detail.</summary>
public static partial class GetPaymentById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single payment by ID using a no-tracking query and maps to full detail response.</summary>
        /// <param name="request">The query containing the payment ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the full payment details or not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null, method found or NotFound returned
            // Load: Payment by ID.
            var payment = await dbContext.Set<PaymentCapture>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Map: Convert to full detail response.
            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                State = payment.State,
                ResponseCode = payment.ResponseCode,
                OrderId = payment.OrderId,
                OrderNumber = null,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethod?.Name,
                CreatedAtUtc = payment.CreatedAtUtc,
                ModifiedAtUtc = payment.ModifiedAtUtc
            };
        }
    }
}

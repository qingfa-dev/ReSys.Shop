using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.Payments;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.Payment.Features.Admin.Payments.Get.ById;

    /// <summary>Handles GetPaymentById feature.</summary>
    public static partial class GetPaymentById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Query: Get payment by ID.
            var payment = await dbContext.Set<PaymentDomain>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentResult.Errors.NotFound;

            // Map: Convert to full detail response.
            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                State = payment.State,
                ResponseCode = payment.ResponseCode,
                IntentClientSecret = payment.IntentClientSecret,
                OrderId = payment.OrderId,
                OrderNumber = null,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = null,
                CreatedAtUtc = payment.CreatedAtUtc,
                ModifiedAtUtc = payment.ModifiedAtUtc
            };
        }
    }
}

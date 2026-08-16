using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Admin.Shared.Mappings;

namespace Module.Billing.Features.Admin.PaymentMethods.Get.ById;

/// <summary>Retrieves a payment method by its unique identifier.</summary>
public static partial class GetPaymentMethodById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads and returns a single payment method by ID using a no-tracking query.</summary>
        /// <param name="request">The query containing the payment method ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the payment method details or not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null, method found or NotFound returned
            // Load: Payment method by ID.
            var method = await dbContext.Set<PaymentMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            // Check: Verify the payment method exists.
            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Map: Return the entity as a response.
            return method.MapToDetail<Response>();
        }
    }
}
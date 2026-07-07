using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Admin.PaymentMethods.Get.ById;

    /// <summary>Handles GetPaymentMethodById feature.</summary>
    public static partial class GetPaymentMethodById
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
            // Query: Retrieve payment method by ID.
            var method = await dbContext.Set<PaymentMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);

            // Check: Verify the payment method exists.
            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Map: Return the entity as a response.
            return method.MapToDetail<Response>();
        }
    }
}

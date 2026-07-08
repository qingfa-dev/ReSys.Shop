using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Payment.Features.Admin.Payments.Get.Paged;

/// <summary>Handles GetPagedPayments feature.</summary>
public static partial class GetPagedPayments
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles the paged query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The paged result.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<PaymentRecord>()
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => new Response
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    State = x.State.ToString(),
                    OrderId = x.OrderId,
                    PaymentMethodId = x.PaymentMethodId
                }, cancellationToken);

            return pagedResult;
        }
    }
}

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Get.Paged;

/// <summary>Retrieves a paged list of payment records with filtering and sorting.</summary>
public static partial class GetPagedPayments
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads and returns paged payment records ordered by default sort with pagination applied.</summary>
        /// <param name="request">The query containing paging and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of payment list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<PaymentCapture>()
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

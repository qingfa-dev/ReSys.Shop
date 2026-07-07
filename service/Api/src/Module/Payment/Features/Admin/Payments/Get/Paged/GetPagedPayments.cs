using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;
using Microsoft.EntityFrameworkCore;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;

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

        // Contract: pre=command!=null, post=result!=null
            // Query: Retrieve payments with order info.
            var query = dbContext.Set<PaymentDomain>()
                .AsNoTracking()
                .ApplyQueryOptions(request.Parameters);

            var pagedResult = await query
                .ToPagedOrAllAsync(x => new Response
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    State = x.State.ToString(),
                    OrderId = x.OrderId,
                    PaymentMethodId = x.PaymentMethodId
                }, request.Parameters, cancellationToken);

            return pagedResult;
        }
    }
}

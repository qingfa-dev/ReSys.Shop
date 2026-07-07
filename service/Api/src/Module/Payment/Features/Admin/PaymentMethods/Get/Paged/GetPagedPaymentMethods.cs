using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Admin.PaymentMethods.Get.Paged;

    /// <summary>Handles GetPagedPaymentMethods feature.</summary>
    public static partial class GetPagedPaymentMethods
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Query: Retrieve all active payment methods with querying options.
            var pagedResult = await dbContext.Set<PaymentMethod>()
                .AsNoTracking()
                .OrderBy(m => m.Position)
                .ThenBy(m => m.Name)
                .ApplyQueryOptions(parameters)
                .ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parameters, cancellationToken);

            return pagedResult;
        }
    }
}

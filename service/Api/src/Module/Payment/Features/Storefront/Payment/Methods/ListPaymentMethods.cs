using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Storefront.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Storefront.Payment.Methods;

public static partial class ListPaymentMethods
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Parse: Query parameters — validates filters, sorting, pagination
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            // Filter: Only active, non-deleted payment methods visible to storefront
            // Load: Paged list with sorting/filtering
            var pagedResult = await dbContext.Set<PaymentMethod>().AsNoTracking()
                .Where(m => m.Active && !m.IsDeleted)
                .ApplyQuerying(parsing.Value)
                .Select(m => m.MapToStoreListItem<Response>())
                .ToPagedOrAllAsync(parsing.Value, x => x, cancellationToken);

            return pagedResult;
        }
    }
}

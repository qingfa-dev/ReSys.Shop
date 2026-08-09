using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Storefront.PaymentMethods.Shared.Mappings;

namespace Module.Billing.Features.Storefront.Payment.Methods;

/// <summary>Lists available payment methods for the customer.</summary>
public static partial class ListPaymentMethods
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Lists available payment methods for the customer.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Parse: Query parameters — validates filters, sorting, pagination
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: PaymentMethodConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: PaymentMethodConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: PaymentMethodConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
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

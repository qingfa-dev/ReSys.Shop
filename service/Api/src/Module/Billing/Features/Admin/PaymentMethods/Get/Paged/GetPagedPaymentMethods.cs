using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Admin.Shared.Mappings;

namespace Module.Billing.Features.Admin.PaymentMethods.Get.Paged;

/// <summary>Retrieves a paged list of payment methods with sorting and filtering.</summary>
public static partial class GetPagedPaymentMethods
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads and returns payment methods ordered by position then name with pagination applied.</summary>
        /// <param name="request">The query containing paging and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of payment method list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Parse: Query parameters — validates filters, sorting, pagination
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: PaymentMethodConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: PaymentMethodConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: PaymentMethodConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Paged payment methods sorted by position then name
            var pagedResult = await dbContext.Set<PaymentMethod>()
                .AsNoTracking()
                .OrderBy(m => m.Position)
                .ThenBy(m => m.Name)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, m => m.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
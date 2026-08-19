using Module.Customer.Domain.Addresses;
using Module.Customer.Features.Shared.Addresses.Mappings;

namespace Module.Customer.Features.Storefront.Addresses.Get.PagedOrAll;

/// <summary>Retrieves paged addresses for the user with filtering, sorting, and search.</summary>
public static partial class GetAddresses
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handles the retrieval of a paged list of addresses for the current user.</summary>
    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Retrieves a paged list of addresses for the current user.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Validate: Parse and validate query parameters for filtering, search, and sort
            var parsing = parameters.ParseAll(
                allowedFilterFields: AddressConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: AddressConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: AddressConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Load: Query addresses scoped to the current user
            var addresses = dbContext.Set<Address>()
                .Include(a => a.UserProfile)
                .Where(a => a.UserProfile!.UserId == request.Parameters.UserId);

            // Transform: Apply paging, filtering, sorting and map to response DTOs
            var pagedResult = await addresses
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(a => a.ToResponse<Response>(), parsing.Value.Page, cancellationToken);

            return pagedResult;
        }
    }
}

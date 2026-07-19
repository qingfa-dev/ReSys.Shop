using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Profile.Features.Store.Addresses.Get.PagedOrAll;

/// <summary>Retrieves paged addresses for the user with filtering, sorting, and search.</summary>
public static partial class GetAddresses
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll(
                allowedFilterFields: AddressConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: AddressConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: AddressConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            var addresses = dbContext.Set<Address>()
                .Where(a => a.UserProfile!.UserId == request.Parameters.UserId);

            var pagedResult = await addresses
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(a => a.ToResponse<Response>(), parsing.Value.Page, cancellationToken);

            return pagedResult;
        }
    }
}

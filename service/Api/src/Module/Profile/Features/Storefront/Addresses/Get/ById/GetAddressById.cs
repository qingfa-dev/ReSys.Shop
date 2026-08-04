using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Shared.Addresses.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Storefront.Addresses.Get.ById;

/// <summary>Retrieves a single address by ID for the authenticated user.</summary>
public static partial class GetAddressById
{
    public sealed record Query(Guid UserId, Guid Id) : IQuery<Response>;

    /// <summary>Handles the retrieval of an address by its identifier.</summary>
    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Retrieves an address by its identifier.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the user's profile from persistence
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            // Validate: Confirm profile exists
            if (profile is null)
                return UserResult.Failure.NotFound;

            // Validate: Confirm address exists on profile
            var address = profile.Addresses.FirstOrDefault(a => a.Id == request.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            // Transform: Map address to response DTO
            return address.ToResponse<Response>();
        }
    }
}

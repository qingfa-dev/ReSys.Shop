using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Get.ById;

/// <summary>Retrieves a single address by ID for the authenticated user.</summary>
public static partial class GetAddressById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads the user profile and returns the matching address or a not-found error.</summary>
        /// <param name="request">The query containing the address ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the address response or an error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=address found or NotFound returned
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return AddressResult.Failure.AuthRequired;

            // Load: Get the profile for the current user
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            // Load: Get the address by its unique identifier
            var address = profile.Addresses.FirstOrDefault(a => a.Id == request.Id);

            if (address is null)
                return AddressResult.Failure.NotFound;

            return address.ToResponse<Response>();
        }
    }
}
using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Store.Addresses.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Get.ById;

public static partial class GetAddressById
{
    // ============ QUERY ============
    public sealed record Query(Guid Id) : IQuery<Response>;

    // ============ QUERY HANDLER ============
    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return AddressResult.Failure.AuthRequired;

            // Resolve: Get the profile for the current user
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            // Resolve: Get the address by its unique identifier
            var address = profile.Addresses.FirstOrDefault(a => a.Id == request.Id);

            if (address is null)
                return AddressResult.Failure.NotFound;

            return address.ToResponse<Response>();
        }
    }
}

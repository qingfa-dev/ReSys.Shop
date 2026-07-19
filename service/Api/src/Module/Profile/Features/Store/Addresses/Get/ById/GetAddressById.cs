using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Get.ById;

/// <summary>Retrieves a single address by ID for the authenticated user.</summary>
public static partial class GetAddressById
{
    public sealed record Query(Guid UserId, Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            var address = profile.Addresses.FirstOrDefault(a => a.Id == request.Id);
            if (address is null)
                return AddressResult.Failure.NotFound;

            return address.ToResponse<Response>();
        }
    }
}

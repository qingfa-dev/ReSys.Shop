using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Shared.Addresses.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public sealed record Query(Guid UserId) : IQuery<Response>;

    internal sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserResult.Failure.NotFound;

            var result = profile.GetDefaultAddress(AddressType.Shipping);
            if (result.IsFailure)
                return AddressResult.Failure.NotFound;

            return result.Value.ToResponse<Response>();
        }
    }
}

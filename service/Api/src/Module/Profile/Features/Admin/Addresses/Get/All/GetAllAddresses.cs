using Module.Profile.Domain;
using Module.Profile.Features.Admin.Addresses.Shared.Mappings;

namespace Module.Profile.Features.Admin.Addresses.Get.All;

public static partial class GetAllAddresses
{
    public sealed record Query(Guid UserId) : IQuery<List<Response>>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var profile = await dbContext.Set<UserProfile>()
                .Include(p => p.Addresses)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
                return UserProfileResult.Failure.UserNotFound;

            return profile.Addresses
                .Select(a => a.ToResponse<Response>())
                .ToList();
        }
    }
}

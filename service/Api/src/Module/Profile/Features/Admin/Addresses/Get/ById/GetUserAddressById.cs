using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Mappings;

namespace Module.Profile.Features.Admin.Addresses.Get.ById;

public static partial class GetUserAddressById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var address = await dbContext.Set<Domain.Addresses.Address>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (address is null)
                return AddressResult.Failure.NotFound;

            return address.ToResponse<Response>();
        }
    }
}

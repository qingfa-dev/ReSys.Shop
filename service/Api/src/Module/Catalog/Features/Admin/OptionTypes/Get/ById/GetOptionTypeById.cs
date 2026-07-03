using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.Get.ById;

/// <summary>
/// Defines the use case for retrieving a single option type by its ID.
/// </summary>
public static partial class GetOptionTypeById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Query: Fetch the option type by its ID.
            var entity = await dbContext.Set<OptionType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity is null)
                return OptionTypeResult.Failure.NotFound;

            // Map: Return the entity as a detail response.
            return entity.MapToDetail<Response>();
        }
    }
}

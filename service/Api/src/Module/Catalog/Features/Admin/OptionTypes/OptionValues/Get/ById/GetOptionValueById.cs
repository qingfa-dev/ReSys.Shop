using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Get.ById;

/// <summary>
/// Defines the use case for retrieving a single option value by its ID.
/// </summary>
public static partial class GetOptionValueById
{
    public sealed record Query(Guid OptionTypeId, Guid Id) : IQuery<Response>;

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
            // Check: Find the specific option value by its ID and parent type ID
            var entity = await dbContext.Set<OptionValue>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.OptionTypeId == request.OptionTypeId, cancellationToken);

            // Guard: Return not found if the entity does not exist
            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            // Map: Transform domain entity to detail response DTO
            return entity.MapToDetail<Response>();
        }
    }
}

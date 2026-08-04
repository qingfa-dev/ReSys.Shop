using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Get.ById;

/// <summary>
/// Defines the use case for retrieving a single option value by its ID.
/// </summary>
public static partial class GetOptionValueById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a single option value by its ID and parent option type ID.
        /// </summary>
        /// <param name="request">The query containing the option value ID and option type ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the option value detail response.</returns>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Find the specific option value by its ID and parent type ID
            var entity = await dbContext.Set<OptionValue>()
                .Include(x => x.OptionType)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Guard: Return not found if the entity does not exist
            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            // Map: Transform domain entity to detail response DTO
            return entity.MapToDetail<Response>();
        }
    }
}
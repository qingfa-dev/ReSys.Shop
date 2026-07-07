using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.Promotions.Create;
/// <summary>Creates a new promotion.</summary>
public static partial class CreatePromotion
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles creating a new promotion.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created promotion response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Map the request to a new Promotion entity.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Failures;

            var promotion = result.Value;

            // Persist: Save the new entity to the database.
            dbContext.Set<Promotion>().Add(promotion);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return Result<Response>.Created(
                promotion.MapToDetail<Response>(),
                PromotionResult.Success.Created(promotion.Id));
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.Promotions.Update;
/// <summary>Updates an existing promotion.</summary>
public static partial class UpdatePromotion
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles updating a promotion.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated promotion response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the promotion exists.
            var promotion = await dbContext.Set<Promotion>().FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
            if (promotion is null)
                return PromotionResult.Errors.NotFound(command.Id);

            // Update: Apply partial changes to the promotion (PATCH semantics).
            var result = command.Request.MapUpdateToDomain(promotion);
            if (result.IsFailure)
                return result.Failures;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return promotion.MapToDetail<Response>();
        }
    }
}

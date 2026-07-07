using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.Services;
using Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.Promotions.Duplicate;
/// <summary>Duplicates an existing promotion with its rules and actions.</summary>
public static partial class DuplicatePromotion
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles duplicating a promotion.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The duplicated promotion response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Query: Load the existing promotion with rules and actions.
            var sourcePromotion = await dbContext.Set<Promotion>()
                .Include(p => p.PromotionRules)
                .Include(p => p.PromotionActions)
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            // Check: Verify the source promotion exists.
            if (sourcePromotion is null)
                return PromotionResult.Errors.NotFound(command.Id);

            // Duplicate: Create a deep copy of the promotion.
            var duplicator = new PromotionDuplicator(sourcePromotion);
            var duplicate = duplicator.Duplicate();

            // Persist: Save the new entity and its related rules/actions to the database.
            dbContext.Set<Promotion>().Add(duplicate);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the duplicated entity as response.
            return duplicate.MapToDetail<Response>();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Features.Admin.Promotions.Delete;
/// <summary>Soft-deletes a promotion by ID.</summary>
public static partial class DeletePromotion
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Handles deleting a promotion.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the promotion exists.
            var promotion = await dbContext.Set<Promotion>().FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
            if (promotion is null)
                return PromotionResult.Errors.NotFound(command.Id);

            // Remove: Soft-delete the promotion.
            var result = promotion.Delete("admin");
            if (result.IsFailure)
                return result.Failures;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}

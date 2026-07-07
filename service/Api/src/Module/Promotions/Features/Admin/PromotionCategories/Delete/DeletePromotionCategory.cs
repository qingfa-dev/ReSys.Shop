using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.PromotionCategories;

namespace Module.Promotions.Features.Admin.PromotionCategories.Delete;
/// <summary>Deletes a promotion category by ID.</summary>
public static partial class DeletePromotionCategory
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Handles deleting a promotion category.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the category exists.
            var category = await dbContext.Set<PromotionCategory>().FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
            if (category is null)
                return PromotionCategoryResult.Errors.NotFound(command.Id);

            // Remove: Delete the category.
            dbContext.Set<PromotionCategory>().Remove(category);

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}

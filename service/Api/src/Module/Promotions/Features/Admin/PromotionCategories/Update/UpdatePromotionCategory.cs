using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionCategories.Update;
/// <summary>Updates an existing promotion category.</summary>
public static partial class UpdatePromotionCategory
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles updating a promotion category.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated promotion category response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the category exists.
            var category = await dbContext.Set<PromotionCategory>().FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
            if (category is null)
                return PromotionCategoryResult.Errors.NotFound(command.Id);

            // Update: Apply partial changes to the category (PATCH semantics).
            var result = command.Request.MapUpdateToDomain(category);
            if (result.IsFailure)
                return result.Failures;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return category.MapToDetail<Response>();
        }
    }
}

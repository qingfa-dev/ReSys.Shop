using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionCategories.Create;
/// <summary>Creates a new promotion category.</summary>
public static partial class CreatePromotionCategory
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles creating a promotion category.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created category response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Map the request to a new PromotionCategory entity.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Failures;

            var category = result.Value;

            // Persist: Save the new entity to the database.
            dbContext.Set<PromotionCategory>().Add(category);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return Result<Response>.Created(
                category.MapToDetail<Response>(),
                PromotionCategoryResult.Success.Created(category.Id));
        }
    }
}

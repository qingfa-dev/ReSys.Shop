using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;

namespace Module.Catalog.Features.Admin.Products.Delete;

/// <summary>
/// Defines the use case for deleting (soft-deleting) a product.
/// </summary>
public static partial class DeleteProduct
{
    public sealed record Command(Guid Id) : ICommand;

    /// <summary>
    /// Soft-deletes a product and cascades the soft-delete to all associated variants.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the deletion command — loads product with variants, soft-deletes
        /// the product and each variant, then persists the state.
        /// </summary>
        /// <param name="command">The command containing the product ID to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A deleted result with the product ID.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=entity.IsDeleted==true && all variants.IsDeleted==true,
        //           throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch product with its variants for cascading soft-delete
            var entity = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return ProductResult.Errors.NotFound(command.Id);

            // Remove: Soft-delete product via domain method
            var deleteResult = entity.Delete(currentUser.UserName ?? "System");
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            // Remove: Cascade soft-delete to all associated variants
            foreach (var variant in entity.Variants)
            {
                variant.Delete(currentUser.UserName ?? "System");
            }

            // Persist: Save soft-delete state to database
            dbContext.Set<Product>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record deletion event for audit trail
            ProductLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok(ProductResult.Success.Deleted(entity.Id));
        }
    }
}

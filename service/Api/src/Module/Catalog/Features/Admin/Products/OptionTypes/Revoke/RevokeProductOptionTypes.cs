using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Revoke;

/// <summary>
/// Defines the use case for revoking option types from a product.
/// </summary>
public static partial class RevokeProductOptionTypes
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Revokes (removes) option type associations from a product by deleting the specified junction records.
        /// </summary>
        /// <param name="command">The command containing the product ID and option type IDs to revoke.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the option types were revoked.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Product exists before removing option type associations
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == command.Id, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(command.Id);

            // Load: Fetch junction entities matching requested option types for this product
            var ids = command.Request.Items.Select(x => x.OptionTypeId).ToList();
            var junctions = await dbContext.Set<ProductOptionType>()
                .Where(x => x.ProductId == command.Id)
                .Where(x => ids.Contains(x.OptionTypeId))
                .ToListAsync(cancellationToken);

            // Skip: No matching associations to remove
            if (junctions.Count == 0)
                return Result.Ok();

            // Remove: Bulk delete all matching junction entities from change tracker
            dbContext.Set<ProductOptionType>().RemoveRange(junctions);

            // Await: Persist removal in single database transaction
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Revocation count for observability
            ProductOptionTypeLoggers.Revoked(logger, ProductId: command.Id, Count: junctions.Count);

            return Result.Ok();
        }
    }
}
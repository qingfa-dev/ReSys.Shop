using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Sync;

public static partial class SyncProductOptionTypes
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Synchronizes product option types by computing the diff between existing and requested assignments,
        /// adding new junctions, updating positions, and removing stale records in a single transaction.
        /// </summary>
        /// <param name="command">The command containing the product ID and the full set of desired option type items.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the option types were synchronized.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Product exists before syncing option types
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == command.Id, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(command.Id);

            // Load: Existing junction records for this product
            var existingJunctions = await dbContext.Set<ProductOptionType>()
                .Where(x => x.ProductId == command.Id)
                .ToListAsync(cancellationToken);

            var requestedItems = command.Request.Items.ToList();
            var existingByOptionTypeId = existingJunctions.ToDictionary(x => x.OptionTypeId);
            var existingIds = existingJunctions.Select(x => x.OptionTypeId).ToHashSet();

            // Update: Position on existing assignments that changed
            foreach (var item in requestedItems)
            {
                if (existingByOptionTypeId.TryGetValue(item.OptionTypeId, out var junction) && junction.Position != item.Position)
                    item.MapToDomain(junction);
            }

            // Compute: Diff for add/remove
            var requestedIds = requestedItems.Select(x => x.OptionTypeId).ToHashSet();
            var toRemove = existingJunctions.Where(x => !requestedIds.Contains(x.OptionTypeId)).ToList();
            var toAdd = requestedItems.Where(x => !existingIds.Contains(x.OptionTypeId)).ToList();

            if (toRemove.Count == 0 && toAdd.Count == 0)
                return Result.Ok();

            // Remove: Stale junction records not in the request
            if (toRemove.Count > 0)
                dbContext.Set<ProductOptionType>().RemoveRange(toRemove);

            // Create: New junction records from assignment items
            foreach (var item in toAdd)
            {
                var result = item.MapToDomain(command.Id);
                if (result.IsFailure)
                    return result.Errors;

                dbContext.Set<ProductOptionType>().Add(result.Value);
            }

            // Await: Persist all changes in a single transaction
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Sync operation summary
            ProductOptionTypeLoggers.Synced(logger, ProductId: command.Id, Added: toAdd.Count, Removed: toRemove.Count);

            return Result.Ok();
        }
    }
}

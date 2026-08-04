using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Assign;

/// <summary>
/// Defines the use case for assigning classifications to a product.
/// </summary>
public static partial class AssignProductClassifications
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Assigns classifications to a product by creating or updating junction records with position tracking.
        /// </summary>
        /// <param name="command">The command containing the product ID and classification items with positions.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the classifications were assigned.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Product exists before assigning classifications
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == command.Id, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(command.Id);

            // Load: Existing junctions for this product
            var existingJunctions = await dbContext.Set<Classification>()
                .Where(x => x.ProductId == command.Id)
                .ToListAsync(cancellationToken);

            var existingByTaxonId = existingJunctions
                .Where(x => x.TaxonId.HasValue)
                .ToDictionary(x => x.TaxonId!.Value);

            var added = 0;
            var updated = 0;
            foreach (var item in command.Request.Items)
            {
                if (existingByTaxonId.TryGetValue(item.TaxonId, out var junction))
                {
                    // Update: Position on existing assignment if changed
                    if (junction.Position != item.Position)
                    {
                        item.MapToDomain(junction);
                        updated++;
                    }
                }
                else
                {
                    // Transform: Assignment item to Classification domain entity
                    var result = item.MapToDomain(command.Id);
                    if (result.IsFailure)
                        return result.Errors;

                    // Add: New junction record to persistence context
                    dbContext.Set<Classification>().Add(result.Value);
                    added++;
                }
            }

            if (added == 0 && updated == 0)
                return Result.Ok();

            // Await: Persist changes to database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Assignment count for observability
            ClassificationLoggers.Assigned(logger, ProductId: command.Id, Count: added);

            return Result.Ok();
        }
    }
}
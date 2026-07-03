using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Assign;

/// <summary>
/// Defines the use case for assigning option types to a product.
/// </summary>
public static partial class AssignProductOptionTypes
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Product exists before assigning option types
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == command.Id, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(command.Id);

            // Load: Existing junctions for this product
            var existingJunctions = await dbContext.Set<ProductOptionType>()
                .Where(x => x.ProductId == command.Id)
                .ToListAsync(cancellationToken);

            var existingByOptionTypeId = existingJunctions.ToDictionary(x => x.OptionTypeId);

            var added = 0;
            var updated = 0;
            foreach (var item in command.Request.Items)
            {
                if (existingByOptionTypeId.TryGetValue(item.OptionTypeId, out var junction))
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
                    // Transform: Assignment item to ProductOptionType domain entity
                    var result = item.MapToDomain(command.Id);
                    if (result.IsFailure)
                        return result.Errors;

                    // Add: New junction record to persistence context
                    dbContext.Set<ProductOptionType>().Add(result.Value);
                    added++;
                }
            }

            if (added == 0 && updated == 0)
                return Result.Ok();

            // Await: Persist changes to database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Assignment count for observability
            ProductOptionTypeLoggers.Assigned(logger, ProductId: command.Id, Count: added);

            return Result.Ok();
        }
    }
}

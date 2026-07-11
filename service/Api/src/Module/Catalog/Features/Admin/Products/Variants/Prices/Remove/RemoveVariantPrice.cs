using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Remove;

/// <summary>
/// Defines the use case for removing a variant price.
/// </summary>
public static partial class RemoveVariantPrice
{
    public sealed record Command(Guid VariantId, Guid PriceId) : ICommand;

    /// <summary>
    /// Soft-deletes a price record for a variant. Validates both the variant
    /// and price exist before performing the soft-delete.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the remove-price command — validates variant and price
        /// existence, soft-deletes the price via domain method, and persists.
        /// </summary>
        /// <param name="command">The command containing the variant ID and price ID to remove.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A deleted result with the price ID.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.VariantId!=Guid.Empty && command.PriceId!=Guid.Empty,
        //           post=price.DeletedAt!=null, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var (variantId, priceId) = command;

            // Load: Verify variant exists before attempting price deletion
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == variantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(variantId);

            // Load: Fetch price by ID and verify it belongs to this variant
            var price = await dbContext.Set<Price>()
                .FirstOrDefaultAsync(p => p.Id == priceId && p.VariantId == variantId, cancellationToken);
            if (price is null)
                return PriceResult.Errors.NotFound;

            // Remove: Soft-delete price via domain method
            var deleteResult = price.Delete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            dbContext.Set<Price>().Update(price);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record price removal event for audit trail
            VariantLoggers.Updated(logger, Sku: string.Empty, Id: variantId, ActionBy: currentUser.UserName);

            return Result.Ok(PriceResult.Success.Deleted(priceId));
        }
    }
}

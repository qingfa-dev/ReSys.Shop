using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Variants.Prices.Set;

/// <summary>
/// Defines the use case for setting (upserting) a variant price.
/// </summary>
public static partial class SetVariantPrice
{
    public sealed record Command(Guid VariantId, Request Request) : ICommand;

    /// <summary>
    /// Sets (upserts) a price for a variant. Looks up an existing price
    /// by variant ID, currency, and country ISO — updates if found, creates if not.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the set-price command — validates variant existence, finds
        /// or creates a price record by (variant, currency, country), and persists.
        /// </summary>
        /// <param name="command">The command containing the variant ID and price request payload.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.VariantId!=Guid.Empty, post=price upserted by (Currency, CountryIso),
        //           throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var (variantId, request) = command;

            // Check: Variant must exist before setting price
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == variantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(variantId);

            // Load: Look for existing price matching variant, currency, and country ISO
            var existing = await dbContext.Set<Price>()
                .FirstOrDefaultAsync(p =>
                    p.VariantId == variantId &&
                    p.Currency == request.Currency &&
                    p.CountryIso == request.CountryIso, cancellationToken);

            if (existing is not null)
            {
                // Update: Apply new amount to existing price entity
                var result = request.MapToDomain(existing);
                if (result.IsFailure)
                    return result.Errors;

                dbContext.Set<Price>().Update(existing);
            }
            else
            {
                // Create: New price entity from request when none exists for this key
                var priceResult = request.MapToDomain(variantId);
                if (priceResult.IsFailure)
                    return priceResult.Errors;

                dbContext.Set<Price>().Add(priceResult.Value);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record price change event for audit trail
            VariantLoggers.Updated(logger, Sku: string.Empty, Id: variantId, ActionBy: currentUser.UserName);

            return Result.Ok(PriceResult.Success.Updated(variantId));
        }
    }
}
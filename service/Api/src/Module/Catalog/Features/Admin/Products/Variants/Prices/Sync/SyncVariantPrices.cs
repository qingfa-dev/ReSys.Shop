using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed record Command(Guid VariantId, Request Request) : ICommand<Response>;

    /// <summary>
    /// Synchronises the full price list for a variant. Performs a three-way
    /// reconciliation: adds new prices, updates existing ones by (Currency, CountryIso),
    /// and soft-deletes prices present in DB but absent from the request.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the sync-prices command — validates variant existence, loads
        /// existing prices, builds a lookup keyed by (Currency, CountryIso),
        /// reconciles add/update/remove, persists atomically.
        /// </summary>
        /// <param name="command">The command containing the variant ID and the full desired price list.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with add/update/remove counts.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.VariantId!=Guid.Empty,
        //           post=prices for variant exactly match request set (add/update/remove reconciled),
        //           throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var (variantId, request) = command;

            // Check: Variant must exist before syncing prices
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == variantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(variantId);

            // Load: Fetch existing (non-deleted) prices for variant
            var existingPrices = await dbContext.Set<Price>()
                .Where(p => p.VariantId == variantId && p.DeletedAt == null)
                .ToListAsync(cancellationToken);

            // Compute: Build lookup keyed by (Currency, CountryIso) for reconciliation
            var existingLookup = existingPrices
                .ToDictionary(p => (Currency: p.Currency, CountryIso: p.CountryIso ?? string.Empty));

            var requestedKeys = request.Prices
                .Select(p => (Currency: p.Currency, CountryIso: p.CountryIso ?? string.Empty))
                .ToHashSet();

            var added = 0;
            var updated = 0;
            var removed = 0;

            // Reconcile: Add or update prices from request
            foreach (var item in request.Prices)
            {
                var key = (Currency: item.Currency, CountryIso: item.CountryIso ?? string.Empty);

                if (existingLookup.TryGetValue(key, out var existing))
                {
                    // Update: Existing price found — apply new amounts
                    var updateResult = existing.Update(
                        amount: item.Amount,
                        compareAtAmount: item.CompareAtAmount,
                        countryIso: item.CountryIso);
                    if (updateResult.IsFailure)
                        return updateResult.Errors;

                    dbContext.Set<Price>().Update(existing);
                    updated++;
                }
                else
                {
                    // Create: New price for this currency/country combination
                    var createResult = PriceMethod.Create(
                        amount: item.Amount,
                        currency: item.Currency,
                        variantId: variantId,
                        compareAtAmount: item.CompareAtAmount,
                        countryIso: item.CountryIso);
                    if (createResult.IsFailure)
                        return createResult.Errors;

                    dbContext.Set<Price>().Add(createResult.Value);
                    added++;
                }
            }

            // Remove: Soft-delete prices that exist in DB but not in request
            foreach (var existing in existingPrices)
            {
                var key = (Currency: existing.Currency, CountryIso: existing.CountryIso ?? string.Empty);
                if (!requestedKeys.Contains(key))
                {
                    var deleteResult = existing.Delete();
                    if (deleteResult.IsFailure)
                        return deleteResult.Errors;

                    dbContext.Set<Price>().Update(existing);
                    removed++;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record price sync summary for audit trail
            VariantLoggers.Updated(logger, Sku: string.Empty, Id: variantId, ActionBy: currentUser.UserName);

            return Result<Response>.Ok(
                new Response { VariantId = variantId, Added = added, Updated = updated, Removed = removed },
                $"Prices synced: {added} added, {updated} updated, {removed} removed.");
        }
    }
}

using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.Values.Sync;

/// <summary>
/// Defines the use case for synchronizing variant option values.
/// </summary>
public static partial class SyncVariantOptionValues
{
    public sealed record Command(Guid VariantId, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Synchronizes variant option values by computing the diff between existing and requested associations,
        /// adding new junction records and removing stale ones in a single transaction.
        /// </summary>
        /// <param name="command">The command containing the variant ID and the full set of desired option value IDs.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the option values were synchronized.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == command.VariantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(command.VariantId);

            var existingJunctions = await dbContext.Set<OptionValueVariant>()
                .Where(x => x.VariantId == command.VariantId)
                .ToListAsync(cancellationToken);

            var existingIds = existingJunctions
                .Select(x => x.OptionValueId)
                .ToHashSet();
            var requestedIds = command.Request.OptionValueIds.ToHashSet();

            var toRemove = existingJunctions
                .Where(x => !requestedIds.Contains(x.OptionValueId))
                .ToList();
            var toAdd = requestedIds.Except(existingIds).ToList();

            // Enforce: a variant can only have one value per option type across the resulting set
            var finalOptionValueIds = existingIds.Except(toRemove.Select(x => x.OptionValueId)).Concat(toAdd).ToList();
            if (finalOptionValueIds.Count > 1)
            {
                var optionTypeByValue = await dbContext.Set<OptionValue>()
                    .Where(x => finalOptionValueIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.OptionTypeId })
                    .ToDictionaryAsync(x => x.Id, x => x.OptionTypeId, cancellationToken);

                var finalOptionTypeIds = finalOptionValueIds
                    .Where(id => optionTypeByValue.ContainsKey(id))
                    .Select(id => optionTypeByValue[id])
                    .ToList();

                var ruleResult = OptionValueVariantMethod.ValidateSingleValuePerOptionType(
                    finalOptionTypeIds, []);
                if (ruleResult.IsFailure)
                    return ruleResult.Errors;
            }

            if (toRemove.Count == 0 && toAdd.Count == 0)
                return Result.Ok();

            if (toRemove.Count > 0)
                dbContext.Set<OptionValueVariant>().RemoveRange(toRemove);

            foreach (var optionValueId in toAdd)
            {
                var junctionResult = OptionValueVariantMethod.Create(command.VariantId, optionValueId);
                if (junctionResult.IsFailure)
                    return junctionResult.Errors;

                dbContext.Set<OptionValueVariant>().Add(junctionResult.Value);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option value sync result for variant audit trail
            OptionValueVariantLoggers.Synced(logger, VariantId: command.VariantId, Added: toAdd.Count, Removed: toRemove.Count);
            return Result.Ok();
        }
    }
}
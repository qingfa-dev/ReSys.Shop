using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Assign;

public static partial class AssignVariantOptionValues
{
    public sealed record Command(Guid VariantId, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Assigns option values to a variant by creating junction records for new associations.
        /// </summary>
        /// <param name="command">The command containing the variant ID and option value IDs to assign.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the option values were assigned.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == command.VariantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(command.VariantId);

            var existingIds = await dbContext.Set<OptionValueVariant>()
                .Where(x => x.VariantId == command.VariantId)
                .Select(x => x.OptionValueId)
                .ToHashSetAsync(cancellationToken);

            var added = 0;
            foreach (var optionValueId in command.Request.OptionValueIds)
            {
                if (existingIds.Contains(optionValueId))
                    continue;

                var junctionResult = OptionValueVariantMethod.Create(command.VariantId, optionValueId);
                if (junctionResult.IsFailure)
                    return junctionResult.Errors;

                dbContext.Set<OptionValueVariant>().Add(junctionResult.Value);
                added++;
            }

            if (added == 0)
                return Result.Ok();

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option value assignment to variant for audit trail
            OptionValueVariantLoggers.Assigned(logger, VariantId: command.VariantId, Count: added);
            return Result.Ok();
        }
    }
}

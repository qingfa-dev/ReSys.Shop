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
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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

                var junctionResult = OptionValueVariantExtensions.Create(command.VariantId, optionValueId);
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

using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed record Command(Guid VariantId, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Revokes (removes) option value associations from a variant by deleting the specified junction records.
        /// </summary>
        /// <param name="command">The command containing the variant ID and option value IDs to revoke.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the option values were revoked.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == command.VariantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(command.VariantId);

            var junctions = await dbContext.Set<OptionValueVariant>()
                .Where(x => x.VariantId == command.VariantId)
                .Where(x => command.Request.OptionValueIds.Contains(x.OptionValueId))
                .ToListAsync(cancellationToken);

            if (junctions.Count == 0)
                return Result.Ok();

            dbContext.Set<OptionValueVariant>().RemoveRange(junctions);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option value revocation from variant for audit trail
            OptionValueVariantLoggers.Revoked(logger, VariantId: command.VariantId, Count: junctions.Count);
            return Result.Ok();
        }
    }
}
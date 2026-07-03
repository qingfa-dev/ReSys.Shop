using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Restore;

namespace Module.Catalog.Features.Admin.Taxonomies.Restore;

/// <summary>
/// Defines the use case for restoring an existing taxonomy.
/// </summary>
public static partial class RestoreTaxonomy
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ISender sender)
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
            // Query: Find the existing taxonomy entity, including its associated taxons.
            var entity = await dbContext.Set<Taxonomy>()
                .IgnoreQueryFilters()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Restore: Restore the taxonomy entity.
            var restoreResult = entity.Restore();
            if (restoreResult.IsFailure)
                return restoreResult.Errors;

            // Persist: Save the changes to the database.
            dbContext.Set<Taxonomy>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Query: Restore the root taxon for this taxonomy.
            var rootTaxon = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TaxonomyId == entity.Id && x.ParentId == null, cancellationToken);
            if (rootTaxon != null)
            {
                await sender.Send(new RestoreTaxon.Command(entity.Id, rootTaxon.Id), cancellationToken);
            }

            // Map: Return the restored response.
            return Result.Ok(TaxonomyResult.Success.Restored);
        }
    }
}

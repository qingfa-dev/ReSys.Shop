using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;

using Module.Catalog.Features.Admin.Taxons.Restore;

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
        /// Restores a soft-deleted taxonomy and its root taxon.
        /// </summary>
        /// <param name="command">The command containing the taxonomy ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=result.IsSuccess, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch soft-deleted taxonomy with associated taxons
            var entity = await dbContext.Set<Taxonomy>()
                .IgnoreQueryFilters()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Update: Restore taxonomy — undeletes entity and resets status
            var restoreResult = entity.Restore();
            if (restoreResult.IsFailure)
                return restoreResult.Errors;

            dbContext.Set<Taxonomy>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Trigger: Cascade restore to root taxon
            var rootTaxon = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TaxonomyId == entity.Id && x.ParentId == null, cancellationToken);
            if (rootTaxon != null)
            {
                await sender.Send(new RestoreTaxon.Command(rootTaxon.Id), cancellationToken);
            }

            // Trigger: Cascade restore to all child taxons
            foreach (var taxon in entity.Taxons.Where(t => t.IsDeleted))
            {
                await sender.Send(new RestoreTaxon.Command(taxon.Id), cancellationToken);
            }

            return Result.Ok(TaxonomyResult.Success.Restored);
        }
    }
}
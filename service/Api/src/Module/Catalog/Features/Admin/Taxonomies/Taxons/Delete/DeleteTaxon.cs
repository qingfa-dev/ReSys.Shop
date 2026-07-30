using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Delete;

/// <summary>
/// Defines the use case for deleting (soft-deleting) a taxon.
/// </summary>
public static partial class DeleteTaxon
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        IAutoClassificationService autoClassificationService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Soft-deletes a taxon after verifying it has no child taxons.
        /// </summary>
        /// <param name="command">The command containing taxonomy ID and taxon ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.TaxonomyId!=Guid.Empty && command.Id!=Guid.Empty, post=result!=null, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var id = command.Id;

            // Load: Fetch taxon with children to validate deletion eligibility
            var entity = await dbContext.Set<Taxon>()
                .Include(x => x.Taxonomy)
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            // Validate: Taxon must exist
            if (entity is null)
                return TaxonResult.Errors.NotFound;
            
            // Enforce: Cannot delete taxon with active children — orphans the hierarchy
            if (entity.Children.Count != 0)
                return TaxonResult.Errors.HasChildren;

            // Remove: Soft-delete the taxon entity
            var deleteResult = entity.Delete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            dbContext.Set<Taxon>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon deletion event for audit trail
            TaxonLoggers.Deleted(logger, entity.Id, entity.Name, entity.TaxonomyId);

            var hierarchyResult = await hierarchyService.RebuildHierarchyAsync(entity.TaxonomyId, null, cancellationToken);
            if (hierarchyResult.IsFailure)
            {
                // Log: Record hierarchy rebuild failure for observability
                TaxonLoggers.HierarchyRebuildFailed(logger, entity.TaxonomyId, entity.Id, hierarchyResult.Errors.FirstOrDefault().Message ?? "Unknown error");
            }
            else
            {
                // Log: Record hierarchy rebuild completion for observability
                TaxonLoggers.HierarchyRebuildFinished(logger, entity.TaxonomyId);
            }

            if (entity.Automatic)
            {
                await autoClassificationService.RegenerateForTaxonAsync(entity.Id, cancellationToken);
            }

            return Result.Ok(TaxonResult.Success.Deleted);
        }
    }
}
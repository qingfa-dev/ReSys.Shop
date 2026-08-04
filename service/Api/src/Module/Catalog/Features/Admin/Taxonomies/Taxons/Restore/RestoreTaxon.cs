using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxons.Restore;

/// <summary>
/// Defines the use case for restoring a soft-deleted taxon.
/// </summary>
public static partial class RestoreTaxon
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Restores a soft-deleted taxon and rebuilds the hierarchy tree.
        /// </summary>
        /// <param name="command">The command containing taxonomy ID and taxon ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.TaxonomyId!=Guid.Empty && command.Id!=Guid.Empty, post=result.IsSuccess, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch soft-deleted taxon (bypassing query filter)
            var entity = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonResult.Errors.NotFound;

            var taxonomyId = entity.TaxonomyId;

            // Update: Restore taxon — undeletes entity and resets status
            var restoreResult = entity.Restore();
            if (restoreResult.IsFailure)
                return restoreResult.Errors;

            dbContext.Set<Taxon>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon restore event for audit trail
            TaxonLoggers.Restored(logger, entity.Id, entity.Name, entity.TaxonomyId);

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

            return Result.Ok(TaxonResult.Success.Updated);
        }
    }
}
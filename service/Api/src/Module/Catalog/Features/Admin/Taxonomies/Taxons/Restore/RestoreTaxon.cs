using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Restore;

/// <summary>
/// Defines the use case for restoring a soft-deleted taxon.
/// </summary>
public static partial class RestoreTaxon
{
    public sealed record Command(Guid TaxonomyId, Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        ILogger<CommandHandler> logger)
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
            var taxonomyId = command.TaxonomyId;

            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == taxonomyId, cancellationToken);
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            var entity = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.TaxonomyId == taxonomyId, cancellationToken);
            if (entity is null)
                return TaxonResult.Errors.NotFound;

            var restoreResult = entity.Restore();
            if (restoreResult.IsFailure)
                return restoreResult.Errors;

            // Persist: Save changes to the database.
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

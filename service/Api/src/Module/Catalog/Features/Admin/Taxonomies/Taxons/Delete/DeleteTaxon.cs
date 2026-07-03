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
    public sealed record Command(Guid TaxonomyId, Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        IAutoClassificationService autoClassificationService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomyId = command.TaxonomyId;
            var id = command.Id;

            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == taxonomyId, cancellationToken);
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            var entity = await dbContext.Set<Taxon>()
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id && x.TaxonomyId == taxonomyId, cancellationToken);
            if (entity is null)
                return TaxonResult.Errors.NotFound;

            if (entity.Children.Count != 0)
                return TaxonResult.Errors.HasChildren;

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

            return new Response { Id = entity.Id };
        }
    }
}

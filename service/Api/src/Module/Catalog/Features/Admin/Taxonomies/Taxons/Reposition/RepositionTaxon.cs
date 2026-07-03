using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Reposition;

/// <summary>
/// Defines the use case for repositioning a taxon.
/// </summary>
public static partial class RepositionTaxon
{
    public sealed record Command(Guid TaxonomyId, Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomyId = command.TaxonomyId;
            var id = command.Id;
            var request = command.Request;

            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == taxonomyId, cancellationToken);
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            var entity = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == id && x.TaxonomyId == taxonomyId, cancellationToken);
            if (entity is null)
                return TaxonResult.Errors.NotFound;

            if (entity.ParentId == null)
                return TaxonResult.Errors.RootLock;

            var oldParentId = entity.ParentId;
            var positionChanged = entity.Position != request.Position;
            var parentChanged = entity.ParentId != request.ParentId;

            if (!positionChanged && !parentChanged)
                return new Response { Id = id };

            if (request.ParentId.HasValue && request.ParentId.Value != entity.ParentId)
            {
                if (request.ParentId.Value == id)
                    return TaxonResult.Errors.SelfParenting;

                var descendantCheck = await hierarchyService.ValidateDescendantAsync(id, request.ParentId.Value, cancellationToken);
                if (descendantCheck.IsFailure)
                    return descendantCheck.Errors;
            }

            var moveResult = entity.Move(request.ParentId, request.Position);
            if (moveResult.IsFailure)
                return moveResult.Errors;

            // Persist: Save changes to the database.
            dbContext.Set<Taxon>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon reposition event for audit trail
            TaxonLoggers.Moved(logger, entity.Name, entity.Id, oldParentId, entity.ParentId, entity.Position);

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

            return new Response { Id = id };
        }
    }
}

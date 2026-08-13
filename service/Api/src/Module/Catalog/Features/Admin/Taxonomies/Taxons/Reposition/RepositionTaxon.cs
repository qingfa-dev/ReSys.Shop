using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxons.Reposition;

/// <summary>
/// Defines the use case for repositioning a taxon.
/// </summary>
public static partial class RepositionTaxon
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Repositions a taxon under a new parent with ancestor-cycle validation and hierarchy rebuild.
        /// </summary>
        /// <param name="command">The command containing taxonomy ID, taxon ID, and new position.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.TaxonomyId!=Guid.Empty && command.Id!=Guid.Empty, post=result.Id!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var request = command.Request;

            // Load: Fetch the taxon to reposition
            var entity = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return TaxonResult.Errors.NotFound;

            var taxonomyId = entity.TaxonomyId;

            // Enforce: Root taxon cannot be repositioned
            if (entity.ParentId == null)
                return TaxonResult.Errors.RootLock;

            var oldParentId = entity.ParentId;
            var positionChanged = entity.Position != request.Position;
            var parentChanged = entity.ParentId != request.ParentId;

            // Skip: No-op when neither position nor parent has changed
            if (!positionChanged && !parentChanged)
                return new Response { Id = id };

            if (request.ParentId.HasValue && request.ParentId.Value != entity.ParentId)
            {
                // Enforce: Taxon cannot be its own parent
                if (request.ParentId.Value == id)
                    return TaxonResult.Errors.SelfParenting;

                // Validate: New parent must not be a descendant of this taxon
                var descendantCheck = await hierarchyService.ValidateDescendantAsync(id, request.ParentId.Value, cancellationToken);
                if (descendantCheck.IsFailure)
                    return descendantCheck.Errors;
            }

            // Update: Apply new parent and position to taxon entity
            var moveResult = entity.Move(request.ParentId, request.Position);
            if (moveResult.IsFailure)
                return moveResult.Errors;

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
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;
using Module.Catalog.Features.Admin.Taxons.Shared.Mappings;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Features.Admin.Taxons.Update;

/// <summary>
/// Defines the use case for updating a taxon.
/// </summary>
public static partial class UpdateTaxon
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Updates a taxon with parent-reparenting validation, ancestor-cycle detection,
        /// and hierarchy rebuild.
        /// </summary>
        /// <param name="command">The command containing taxonomy ID, taxon ID, and update payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.TaxonomyId!=Guid.Empty && command.Id!=Guid.Empty, post=result!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var request = command.Request;

            // Load: Fetch the taxon to update
            var entity = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return TaxonResult.Errors.NotFound;

            var taxonomyId = entity.TaxonomyId;

            if (request.ParentId.HasValue && request.ParentId.Value != entity.ParentId)
            {
                // Enforce: Root taxon cannot be reparented
                if (entity.ParentId == null)
                    return TaxonResult.Errors.RootLock;

                // Enforce: Taxon cannot be its own parent — creates circular reference
                if (request.ParentId.Value == id)
                    return TaxonResult.Errors.SelfParenting;

                // Validate: New parent must not be a descendant of this taxon
                var descendantCheck = await hierarchyService.ValidateDescendantAsync(id, request.ParentId.Value, cancellationToken);
                if (descendantCheck.IsFailure)
                    return descendantCheck.Errors;
            }

            // Validate: Taxon name must be unique at the target parent level
            var targetParentId = request.ParentId ?? entity.ParentId;
            var normalizedName = ParameterizableBehavior.Normalize(request.Name);
            var nameExists = await dbContext.Set<Taxon>()
                .AnyAsync(x => x.TaxonomyId == taxonomyId &&
                               x.ParentId == targetParentId &&
                               x.Name == normalizedName &&
                               x.Id != id, cancellationToken);
            if (nameExists)
                return TaxonResult.Errors.DuplicateName;

            var oldParentId = entity.ParentId;
            var oldPosition = entity.Position;
            var oldName = entity.Name;

            // Update: Apply incoming values to existing taxon entity
            var updateResult = request.MapToDomain(entity);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            dbContext.Set<Taxon>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon update event for audit trail
            TaxonLoggers.Updated(logger, entity.Id, entity.Name, entity.TaxonomyId);
            if (oldParentId != entity.ParentId)
            {
                // Log: Record taxon reposition event for audit trail
                TaxonLoggers.Moved(logger, entity.Name, entity.Id, oldParentId, entity.ParentId, entity.Position);
            }

            // Call: Rebuild hierarchy for the taxonomy.
            await hierarchyService.RebuildHierarchyAsync(entity.TaxonomyId, null, cancellationToken);

            // Map: Return updated response.
            return Result<Response>.Ok(entity.MapToDetail<Response>(), TaxonResult.Success.Updated);
        }
    }
}
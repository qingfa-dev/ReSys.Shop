using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;

/// <summary>
/// Defines the use case for updating a taxon.
/// </summary>
public static partial class UpdateTaxon
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

            if (request.ParentId.HasValue && request.ParentId.Value != entity.ParentId)
            {
                if (entity.ParentId == null)
                    return TaxonResult.Errors.RootLock;

                if (request.ParentId.Value == id)
                    return TaxonResult.Errors.SelfParenting;

                var descendantCheck = await hierarchyService.ValidateDescendantAsync(id, request.ParentId.Value, cancellationToken);
                if (descendantCheck.IsFailure)
                    return descendantCheck.Errors;
            }

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

            var updateResult = request.MapToDomain(entity);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Persist: Save changes to the database.
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

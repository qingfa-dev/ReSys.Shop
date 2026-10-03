using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;
using Module.Catalog.Features.Admin.Shared.Mappings;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Features.Admin.Taxons.Create;

/// <summary>
/// Defines the use case for creating a new taxon.
/// </summary>
public static partial class CreateTaxon
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ITaxonHierarchyService hierarchyService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Creates a new taxon under a parent, validates parent ancestry and name uniqueness,
        /// then rebuilds the hierarchy tree.
        /// </summary>
        /// <param name="command">The command containing parent taxonomy ID and taxon payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.TaxonomyId!=Guid.Empty, post=result.Id!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var taxonomyId = request.TaxonomyId;

            // Validate: Parent taxonomy must exist to accept new taxons
            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == taxonomyId, cancellationToken);
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            if (request.ParentId.HasValue)
            {
                // Validate: Parent taxon must exist and belong to same taxonomy
                var parent = await dbContext.Set<Taxon>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);

                if (parent is null)
                    return TaxonResult.Errors.InvalidParent;

                if (parent.TaxonomyId != taxonomyId)
                    return TaxonResult.Errors.ParentTaxonomyMismatch;
            }

            // Validate: Taxon name must be unique within its parent at the same hierarchy level
            var normalizedName = ParameterizableBehavior.Normalize(request.Name);
            var nameExists = await dbContext.Set<Taxon>()
                .AnyAsync(x => x.TaxonomyId == taxonomyId &&
                               x.ParentId == request.ParentId &&
                               x.Name == normalizedName, cancellationToken);
            if (nameExists)
                return TaxonResult.Errors.DuplicateName;

            // Create: Instantiate new taxon entity from validated request
            var result = request.MapToDomain(taxonomyId);
            if (result.IsFailure)
                return result.Errors;
            var entity = result.Value;

            dbContext.Set<Taxon>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon creation event for audit trail
            TaxonLoggers.Created(logger, entity.Id, entity.Name, entity.TaxonomyId);

            // Trigger: Rebuild hierarchy tree to reflect new taxon position
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

            // Map: Return created response.
            return Result<Response>.Created(
                entity.MapToDetail<Response>(),
                TaxonResult.Success.Created);
        }
    }
}
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Create;

/// <summary>
/// Defines the use case for creating a new taxon.
/// </summary>
public static partial class CreateTaxon
{
    public sealed record Command(Guid TaxonomyId, Request Request) : ICommand<Response>;

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
            var request = command.Request;

            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == taxonomyId, cancellationToken);
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            if (request.ParentId.HasValue)
            {
                var parent = await dbContext.Set<Taxon>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);

                if (parent is null)
                    return TaxonResult.Errors.InvalidParent;

                if (parent.TaxonomyId != taxonomyId)
                    return TaxonResult.Errors.ParentTaxonomyMismatch;
            }

            var normalizedName = ParameterizableBehavior.Normalize(request.Name);
            var nameExists = await dbContext.Set<Taxon>()
                .AnyAsync(x => x.TaxonomyId == taxonomyId &&
                               x.ParentId == request.ParentId &&
                               x.Name == normalizedName, cancellationToken);
            if (nameExists)
                return TaxonResult.Errors.DuplicateName;

            var result = request.MapToDomain(taxonomyId);
            if (result.IsFailure)
                return result.Errors;
            var entity = result.Value;

            // Persist: Add entity to database and save changes.
            dbContext.Set<Taxon>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon creation event for audit trail
            TaxonLoggers.Created(logger, entity.Id, entity.Name, entity.TaxonomyId);

            // Call: Rebuild hierarchy for the taxonomy.
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

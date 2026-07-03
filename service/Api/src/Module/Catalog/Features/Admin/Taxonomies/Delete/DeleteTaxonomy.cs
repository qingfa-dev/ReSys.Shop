using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Delete;

namespace Module.Catalog.Features.Admin.Taxonomies.Delete;

/// <summary>
/// Defines the use case for deleting an existing taxonomy.
/// </summary>
public static partial class DeleteTaxonomy
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ISender sender)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Query: Find the existing taxonomy entity, including its associated taxons.
            var entity = await dbContext.Set<Taxonomy>()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Guard: Prevent deletion if there are any associated taxons.
            // Requirement: Allow deletion if no taxons left or only the root with no children left.
            var nonRootTaxonsCount = entity.Taxons.Count(x => x.ParentId != null);
            var rootTaxon = entity.Taxons.FirstOrDefault(x => x.ParentId == null);

            if (nonRootTaxonsCount > 0 || (rootTaxon != null && rootTaxon.Children.Count > 0))
                return TaxonomyResult.Errors.HasTaxons;

            // Remove: Delete the taxonomy entity from the database.
            var deleteResult = entity.Delete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            // Remove: Delete the taxonomy entity from the database.
            dbContext.Set<Taxonomy>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Root taxon: Delete the root taxon for this taxonomy.
            var root = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TaxonomyId == entity.Id && x.ParentId == null, cancellationToken);
            if (root != null)
            {
                await sender.Send(new DeleteTaxon.Command(entity.Id, root.Id), cancellationToken);
            }

            // Create: The response with the ID of the deleted entity.
            return Result.Ok(TaxonomyResult.Success.Deleted);
        }
    }
}

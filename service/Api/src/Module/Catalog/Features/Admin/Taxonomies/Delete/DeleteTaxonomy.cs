using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;

using Module.Catalog.Features.Admin.Taxons.Delete;

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
        /// Deletes a taxonomy after ensuring no active child taxon remain.
        /// </summary>
        /// <param name="command">The command containing the taxonomy ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=result.IsSuccess, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch taxonomy with associated taxon to evaluate deletion eligibility
            var entity = await dbContext.Set<Taxonomy>()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Enforce: Prevent deletion when non-root taxon or root with children still exist
            var nonRootTaxonsCount = entity.Taxons.Count(x => x.ParentId != null);
            var rootTaxon = entity.Taxons.FirstOrDefault(x => x.ParentId == null);

            if (nonRootTaxonsCount > 0 || (rootTaxon != null && rootTaxon.Children.Count > 0))
                return TaxonomyResult.Errors.HasTaxons;

            // Remove: Soft-delete the taxonomy entity
            var deleteResult = entity.Delete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            dbContext.Set<Taxonomy>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Trigger: Cascade delete to root taxon via DeleteTaxon command
            var root = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TaxonomyId == entity.Id && x.ParentId == null, cancellationToken);
            if (root != null)
            {
                await sender.Send(new DeleteTaxon.Command(root.Id), cancellationToken);
            }

            return Result.Ok(TaxonomyResult.Success.Deleted);
        }
    }
}
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Create;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Restore;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Features.Admin.Taxonomies.Update;

/// <summary>
/// Defines the use case for updating a taxonomy.
/// </summary>
public static partial class UpdateTaxonomy
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ISender sender)
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
            var request = command.Request;

            // Query: Find the existing taxonomy entity by its ID.
            var entity = await dbContext.Set<Taxonomy>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Validate: Ensure that no other taxonomy with the same name exists.
            var normalizedName = ParameterizableBehavior.Normalize(request.Name);
            var nameExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Name == normalizedName && x.Id != command.Id, cancellationToken);
            if (nameExists)
                return TaxonomyResult.Errors.DuplicateName;

            // Update: Apply the changes from the request to the domain entity.
            var updateResult = request.MapToDomain(entity);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Persist: Save the changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Query: Find the root taxon for this taxonomy (including soft-deleted).
            var rootTaxon = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TaxonomyId == entity.Id && x.ParentId == null, cancellationToken);

            if (rootTaxon == null)
            {
                // Create: Root taxon if it's missing.
                var createRequest = new CreateTaxon.Request
                {
                    Name = entity.Name,
                    Presentation = entity.Presentation,
                    Slug = entity.Name.ToLowerInvariant(),
                    Position = 0
                };
                await sender.Send(new CreateTaxon.Command(entity.Id, createRequest), cancellationToken);
            }
            else
            {
                if (rootTaxon.IsDeleted)
                {
                    // Restore: If the root taxon was soft-deleted, restore it first.
                    await sender.Send(new RestoreTaxon.Command(entity.Id, rootTaxon.Id), cancellationToken);
                }

                // Update: Existing root taxon with the new taxonomy name and presentation.
                var updateRequest = new UpdateTaxon.Request
                {
                    Name = entity.Name,
                    Presentation = entity.Presentation,
                    Slug = entity.Name.ToLowerInvariant(),
                    Position = rootTaxon.Position
                };
                await sender.Send(new UpdateTaxon.Command(entity.Id, rootTaxon.Id, updateRequest), cancellationToken);
            }

            // Map: Convert the updated entity to a detailed response DTO.
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                TaxonomyResult.Success.Updated);
        }
    }
}

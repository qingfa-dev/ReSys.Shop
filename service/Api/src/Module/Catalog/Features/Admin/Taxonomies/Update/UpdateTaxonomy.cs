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
        /// Updates a taxonomy and synchronizes its root taxon (create, restore, or rename).
        /// </summary>
        /// <param name="command">The command containing the taxonomy ID and update payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty && command.Request!=null, post=result!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load: Fetch the existing taxonomy entity to modify
            var entity = await dbContext.Set<Taxonomy>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Validate: Updated taxonomy name must not conflict with another entity
            var normalizedName = ParameterizableBehavior.Normalize(request.Name);
            var nameExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Name == normalizedName && x.Id != command.Id, cancellationToken);
            if (nameExists)
                return TaxonomyResult.Errors.DuplicateName;

            // Update: Apply incoming request values to existing taxonomy entity
            var updateResult = request.MapToDomain(entity);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Load: Find root taxon (including soft-deleted) to determine sync action
            var rootTaxon = await dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TaxonomyId == entity.Id && x.ParentId == null, cancellationToken);

            if (rootTaxon == null)
            {
                // Trigger: Create missing root taxon to maintain taxonomy integrity
                var createRequest = new CreateTaxon.Request
                {
                    Name = entity.Name,
                    Presentation = entity.Presentation,
                    Slug = entity.Name.ToLowerInvariant(),
                    Position = 0,
                    TaxonomyId = entity.Id
                };
                await sender.Send(new CreateTaxon.Command(createRequest), cancellationToken);
            }
            else
            {
                if (rootTaxon.IsDeleted)
                {
                    // Trigger: Restore soft-deleted root taxon before applying updates
                    await sender.Send(new RestoreTaxon.Command(rootTaxon.Id), cancellationToken);
                }

                // Trigger: Sync root taxon with updated taxonomy name and presentation
                var updateRequest = new UpdateTaxon.Request
                {
                    Name = entity.Name,
                    Presentation = entity.Presentation,
                    Slug = entity.Name.ToLowerInvariant(),
                    Position = rootTaxon.Position
                };
                await sender.Send(new UpdateTaxon.Command(rootTaxon.Id, updateRequest), cancellationToken);
            }

            // Map: Convert the updated entity to a detailed response DTO.
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                TaxonomyResult.Success.Updated);
        }
    }
}
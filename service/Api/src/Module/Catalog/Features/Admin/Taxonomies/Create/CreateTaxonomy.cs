using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Create;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Features.Admin.Taxonomies.Create;

/// <summary>
/// Defines the use case for creating a new taxonomy.
/// </summary>
public static partial class CreateTaxonomy
{
    public record Command(Request Request) : ICommand<Response>;

    public class CommandHandler(
        IApplicationDbContext dbContext,
        ISender sender
        ) : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Creates a new taxonomy with a root taxon after validating name uniqueness.
        /// </summary>
        /// <param name="command">The command containing the taxonomy creation payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Request!=null, post=result.Id!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Validate: Taxonomy name must be unique to prevent duplicate URL routes
            var normalizedName = ParameterizableBehavior.Normalize(request.Name);
            var nameExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Name == normalizedName, cancellationToken);
            if (nameExists)
                return TaxonomyResult.Errors.DuplicateName;

            // Create: Instantiate a new taxonomy entity from the validated request
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            dbContext.Set<Taxonomy>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Call: Dispatch CreateTaxon command to create root taxon.
            var taxonRequest = new CreateTaxon.Request
            {
                Name = entity.Name,
                Presentation = entity.Presentation,
                Slug = ProductMethod.GenerateSlugFromName(entity.Name),
                Position = 0
            };
            await sender.Send(new CreateTaxon.Command(entity.Id, taxonRequest), cancellationToken);

            // Map: Return created response.
            return Result<Response>.Created(
                entity.MapToDetail<Response>(),
                TaxonomyResult.Success.Created);
        }
    }
}

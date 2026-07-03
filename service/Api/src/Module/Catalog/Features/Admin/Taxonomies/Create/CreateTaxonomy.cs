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
    // Command:
    public record Command(Request Request) : ICommand<Response>;

    // Command Handler:
    public class CommandHandler(
        IApplicationDbContext dbContext,
        ISender sender
        ) : ICommandHandler<Command, Response>
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
            // Normalize: Apply any necessary normalization to the request data (e.g., trimming whitespace).
            var normalizedName = ParameterizableBehavior.Normalize(request.Name);

            // Validate: Ensure that a taxonomy with the same name does not already exist.
            var nameExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Name == normalizedName, cancellationToken);
            if (nameExists)
                return TaxonomyResult.Errors.DuplicateName;

            // Create: Instantiate a new taxonomy entity from the request.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            // Persist: Add entity to database and save changes.
            dbContext.Set<Taxonomy>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Call: Dispatch CreateTaxon command to create root taxon.
            var taxonRequest = new CreateTaxon.Request
            {
                Name = entity.Name,
                Presentation = entity.Presentation,
                Slug = entity.Name.ToLowerInvariant(),
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

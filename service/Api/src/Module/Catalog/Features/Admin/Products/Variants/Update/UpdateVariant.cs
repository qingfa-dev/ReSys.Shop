using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.Update;

/// <summary>
/// Defines the use case for updating a variant.
/// </summary>
public static partial class UpdateVariant
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    /// <summary>
    /// Updates an existing variant's fields, pricing, and physical specifications.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the update-variant command — loads the variant with prices,
        /// applies request fields via domain mapping, persists, and returns
        /// the updated variant detail.
        /// </summary>
        /// <param name="command">The command containing the variant ID and update request payload.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the updated variant detail.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=variant fields updated, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var (id, request) = command;

            // Load: Fetch existing variant with prices for update
            var entity = await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return VariantResult.Errors.NotFound(id);

            // Check: SKU must be unique across all variants (excluding current variant)
            var skuExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Sku == request.Sku && x.Id != command.Id, cancellationToken);

            if (skuExists)
                return VariantResult.Errors.SkuAlreadyExists(request.Sku);

            // Update: Apply request fields to existing variant domain entity via mapping
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record variant update event for observability
            VariantLoggers.Updated(logger, Sku: entity.Sku!, Id: entity.Id, ActionBy: currentUser.UserName);

            // Map: Return updated variant as detail DTO
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                VariantResult.Success.Updated(entity.Id));
        }
    }
}
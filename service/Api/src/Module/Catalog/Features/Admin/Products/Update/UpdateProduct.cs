using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;
using Module.Catalog.Features.Admin.Variants.Update;

namespace Module.Catalog.Features.Admin.Products.Update;

/// <summary>
/// Defines the use case for updating a product.
/// </summary>
public static partial class UpdateProduct
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    /// <summary>
    /// Updates an existing product and its master variant. Validates slug uniqueness
    /// against other products, applies request fields to the domain entity,
    /// and dispatches a master variant update command.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the update command — loads product with variants, validates slug,
        /// applies domain mapping, persists, and updates master variant.
        /// </summary>
        /// <param name="command">The command containing the product ID and update request payload.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the updated product detail.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=product fields updated, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var (id, request) = command;

            // Load: Fetch existing product with variants for update
            var entity = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return ProductResult.Errors.NotFound(id);

            // Validate: New slug must not conflict with another product
            var slugExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Slug == request.Slug && x.Id != id, cancellationToken);
            if (slugExists)
                return ProductResult.Errors.DuplicateSlug;

            // Update: Apply request fields to existing domain entity via mapping
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Trigger: Update master variant via command (mirrors CreateProduct → AddVariant pattern)
            var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);
            if (masterVariant is not null)
            {
                var variantRequest = new UpdateVariant.Request
                {
                    Sku = $"{entity.Slug}-master",
                    TrackInventory = masterVariant.TrackInventory,
                };

                var variantResult = await sender.Send(
                    new UpdateVariant.Command(masterVariant.Id, variantRequest), cancellationToken);
                if (variantResult.IsFailure)
                    return variantResult.Errors;
            }

            // Log: Record product update event for observability
            ProductLoggers.Updated(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            // Map: Return updated product as detail DTO
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                ProductResult.Success.Updated(entity.Id));
        }
    }
}
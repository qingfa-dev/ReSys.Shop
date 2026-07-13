using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Activate;

/// <summary>
/// Defines the use case for activating a product.
/// </summary>
public static partial class ActivateProduct
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    /// <summary>
    /// Activates a product by setting its status to Active and assigning an
    /// available-on timestamp if not already set.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the activation command — loads the product, updates status,
        /// persists the change, and returns the updated product detail.
        /// </summary>
        /// <param name="command">The command containing the product ID to activate.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the activated product detail.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=entity.Status==Active, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch product by ID to verify existence before status change
            var entity = await dbContext.Set<Product>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return ProductResult.Errors.NotFound(command.Id);

            // Update: Set product to Active status with available-on timestamp
            entity.AvailableOn ??= DateTimeOffset.UtcNow;
            entity.Status = ProductStatus.Active;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record activation event for observability
            ProductLoggers.StatusChanged(logger, Name: entity.Name, Id: entity.Id, NewStatus: entity.Status, ActionBy: currentUser.UserName);

            // Map: Return updated product as detail DTO
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                ProductResult.Success.Updated(entity.Id));
        }
    }
}
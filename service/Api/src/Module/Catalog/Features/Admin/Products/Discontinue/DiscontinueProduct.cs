using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Discontinue;

/// <summary>
/// Defines the use case for discontinuing (archiving) a product.
/// </summary>
public static partial class DiscontinueProduct
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    /// <summary>
    /// Discontinues (archives) a product by setting its status to Archived
    /// and assigning a discontinue-on timestamp if not already set.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the discontinuation command — loads the product, sets status
        /// to Archived, persists, and returns the updated detail.
        /// </summary>
        /// <param name="command">The command containing the product ID to discontinue.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the archived product detail.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=entity.Status==Archived, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch product by ID to verify existence before discontinuation
            var entity = await dbContext.Set<Product>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
                return ProductResult.Errors.NotFound(command.Id);

            // Update: Archive product with discontinue-on timestamp
            entity.DiscontinueOn ??= DateTimeOffset.UtcNow;
            entity.Status = ProductStatus.Archived;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record discontinuation event for observability
            ProductLoggers.StatusChanged(logger, Name: entity.Name, Id: entity.Id, NewStatus: entity.Status, ActionBy: currentUser.UserName);

            // Map: Return archived product as detail DTO
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                ProductResult.Success.Updated(entity.Id));
        }
    }
}

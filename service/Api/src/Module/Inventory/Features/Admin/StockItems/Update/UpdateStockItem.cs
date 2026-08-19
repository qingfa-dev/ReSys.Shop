using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Update;

/// <summary>Updates an existing stock item's quantity and properties after verifying it exists.</summary>
public static partial class UpdateStockItem
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates the stock item properties from the request mapping.</summary>
        /// <param name="command">The command containing update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the updated stock item details.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var request = command.Request;

            // Check: Find the existing entity.
            var entity = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return StockItemResult.Errors.NotFound(command.Id);

            // Update: Map the request to update the entity.
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock item updated.
            StockItemLoggers.Adjusted(logger, CountOnHand: entity.CountOnHand, Id: entity.Id, ActionBy: currentUser.UserName);
            // Map: Return the updated stock item as response.
            return entity.MapToDetail<Response>();
        }
    }
}
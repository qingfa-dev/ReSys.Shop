using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Delete;

/// <summary>Soft-deletes a stock item after verifying it exists and removing its state.</summary>
public static partial class DeleteStockItem
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Removes the stock item by identifier and persists the change.</summary>
        /// <param name="command">The command containing the stock item identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the deleted stock item response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Load: Find the stock item by identifier
            var item = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (item is null)
                return StockItemResult.Errors.NotFound(command.Id);

            var deleted = item.MapToListItem<Response>();

            dbContext.Set<StockItem>().Remove(item);
            await dbContext.SaveChangesAsync(cancellationToken);

            return deleted;
        }
    }
}
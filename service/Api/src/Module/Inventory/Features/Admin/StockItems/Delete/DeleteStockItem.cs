using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Features.Admin.StockItems.Delete;
/// <summary>Handles deletion of a stock item.</summary>
public static partial class DeleteStockItem
{
    public sealed record Command(Guid Id) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Executes the delete stock item command.</summary>
        /// <param name="command">The command containing the stock item identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Find the stock item by identifier.
            var item = await dbContext.Set<StockItem>().FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (item is null) return StockItemResult.Errors.NotFound(command.Id);
            // Remove: Delete the stock item from the database.
            dbContext.Set<StockItem>().Remove(item);
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}

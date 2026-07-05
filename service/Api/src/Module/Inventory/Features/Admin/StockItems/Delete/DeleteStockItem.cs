using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Delete;

public static partial class DeleteStockItem
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
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

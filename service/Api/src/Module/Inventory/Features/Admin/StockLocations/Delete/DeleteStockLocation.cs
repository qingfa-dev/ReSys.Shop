using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.Delete;

public static partial class DeleteStockLocation
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return StockLocationResult.Failure.NotFound;

            if (entity.Active)
                return StockLocationResult.Failure.CannotDeleteActive;

            if (entity.Default)
                return StockLocationResult.Failure.CannotDeactivateDefault;

            var deleteResult = entity.SoftDelete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            StockLocationLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            return entity.MapToListItem<Response>();
        }
    }
}
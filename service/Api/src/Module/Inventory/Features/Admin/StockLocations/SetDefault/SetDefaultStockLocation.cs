using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.SetDefault;

public static partial class SetDefaultStockLocation
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

            var currentDefault = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Default && x.Id != command.Id, cancellationToken);

            if (currentDefault is not null)
            {
                currentDefault.Default = false;
            }

            var result = entity.SetAsDefault();
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            StockLocationLoggers.SetAsDefault(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            return entity.MapToDetail<Response>();
        }
    }
}
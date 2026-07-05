using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.SetDefault;

/// <summary>Handles setting a stock location as the default.</summary>
public static partial class SetDefaultStockLocation
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the set default stock location command.</summary>
        /// <param name="command">The command containing the stock location identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Find the entity to set as default.
            var entity = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return StockLocationResult.Errors.NotFound;

            // Check: Unset any existing default stock location.
            var currentDefault = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Default && x.Id != command.Id, cancellationToken);

            if (currentDefault is not null)
            {
                currentDefault.Default = false;
            }

            // Update: Set the new default.
            var result = entity.SetAsDefault();
            if (result.IsFailure)
                return result.Errors;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock location set as default.
            StockLocationLoggers.SetAsDefault(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            // Map: Return success result.
            return Result.Ok(StockLocationResult.Success.SetAsDefault);
        }
    }
}

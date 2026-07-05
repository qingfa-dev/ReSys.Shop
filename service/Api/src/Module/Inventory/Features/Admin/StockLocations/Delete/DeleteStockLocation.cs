using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Delete;

/// <summary>Handles deletion of a stock location.</summary>
public static partial class DeleteStockLocation
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the delete stock location command.</summary>
        /// <param name="command">The command containing the stock location identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Find the existing entity.
            var entity = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return StockLocationResult.Errors.NotFound;

            // Validate: Cannot delete active stock location.
            if (entity.Active)
                return StockLocationResult.Errors.CannotDeleteActive;

            // Check: Cannot delete default stock location.
            if (entity.Default)
                return StockLocationResult.Errors.CannotDeactivateDefault;

            // Remove: Soft-delete the entity.
            var deleteResult = entity.SoftDelete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock location deleted.
            StockLocationLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            // Map: Return success result.
            return Result.NoContent(StockLocationResult.Success.Deleted);
        }
    }
}

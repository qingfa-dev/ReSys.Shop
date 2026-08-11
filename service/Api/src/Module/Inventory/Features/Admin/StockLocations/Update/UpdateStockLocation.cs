using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.Update;

/// <summary>Handles update of an existing stock location.</summary>
public static partial class UpdateStockLocation
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the update stock location command.</summary>
        /// <param name="command">The command containing the update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the updated stock location details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Find the existing entity.
            var entity = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return StockLocationResult.Errors.NotFound;

            // Update: Map the request to update the entity.
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock location updated.
            StockLocationLoggers.Updated(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);
            // Map: Return the updated stock location as response.
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                StockLocationResult.Success.Updated);
        }
    }
}
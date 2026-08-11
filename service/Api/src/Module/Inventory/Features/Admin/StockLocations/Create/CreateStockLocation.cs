using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.Create;

/// <summary>Handles creation of a new stock location.</summary>
public static partial class CreateStockLocation
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the create stock location command.</summary>
        /// <param name="command">The command containing the stock location request data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success with the created response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Verify if a stock location with the same name already exists.
            var nameExists = await dbContext.Set<StockLocation>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (nameExists)
                return StockLocationResult.Errors.DuplicateName;

            // Create: Map the request to a new StockLocation entity.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            dbContext.Set<StockLocation>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock location created.
            StockLocationLoggers.Created(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);
            // Map: Return the created stock location as response.
            return Result<Response>.Created(
                entity.MapToDetail<Response>(),
                StockLocationResult.Success.Created);
        }
    }
}
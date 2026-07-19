using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.SetDefault;

public static partial class SetDefaultStockLocation
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    /// <summary>Handler for setting a default stock location.</summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Sets a stock location as the default.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch the target stock location
            var entity = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            // Check: Reject if location does not exist
            if (entity is null)
                return StockLocationResult.Failure.NotFound;

            // Load: Find the currently-default location to unmark it
            var currentDefault = await dbContext.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Default && x.Id != command.Id, cancellationToken);

            if (currentDefault is not null)
            {
                // Update: Unmark the previous default location
                currentDefault.Default = false;
            }

            // Update: Set the new default and validate transition
            var result = entity.SetAsDefault();
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record default location change for audit trail
            StockLocationLoggers.SetAsDefault(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            // Transform: Map domain entity to response DTO
            return entity.MapToDetail<Response>();
        }
    }
}
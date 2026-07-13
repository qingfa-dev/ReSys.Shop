using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockTransfers.Create;

/// <summary>Creates a stock transfer request between source and destination locations in Draft state.</summary>
public static partial class CreateStockTransfer
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Maps the request to a domain transfer entity and persists it.</summary>
        /// <param name="command">The command containing transfer data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the created transfer details.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var request = command.Request;

            // Map: Convert request to domain entity via shared mapper
            var createResult = request.MapToDomain();

            // Create: Build domain entity from factory
            if (createResult.IsFailure)
                return createResult.Errors;

            var transfer = createResult.Value;
            transfer.CreatedBy = currentUser.UserName;

            dbContext.Set<StockTransfer>().Add(transfer);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock transfer created
            StockTransferLoggers.Created(logger, Id: transfer.Id, SourceId: transfer.SourceLocationId, DestinationId: transfer.DestinationLocationId);

            // Map: Return the created transfer as response
            return transfer.MapToDetail<Response>();
        }
    }
}
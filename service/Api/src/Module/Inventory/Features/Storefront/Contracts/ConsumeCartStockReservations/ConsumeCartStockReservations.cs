using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Shared.Application.Contracts.Inventory;

namespace Module.Inventory.Features.Storefront.Contracts.ConsumeCartStockReservations;

public sealed class ConsumeCartStockReservationsCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ConsumeCartStockReservationsCommand, ConsumeCartStockReservationsResponse>
{
    public async Task<Result<ConsumeCartStockReservationsResponse>> Handle(
        ConsumeCartStockReservationsCommand command, CancellationToken cancellationToken)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == command.CartId.ToString()
                        && r.State == ReservationState.Reserved)
            .ToListAsync(cancellationToken);

        if (reservations.Count == 0)
            return new ConsumeCartStockReservationsResponse
            {
                Success = false,
                ErrorMessage = "No active reservations found - reservations may have expired"
            };

        foreach (var reservation in reservations)
        {
            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(
                    si => si.VariantId == reservation.VariantId
                          && si.StockLocationId == reservation.StockLocationId,
                    cancellationToken);

            if (stockItem is null)
            {
                return new ConsumeCartStockReservationsResponse
                {
                    Success = false,
                    ErrorMessage = $"Stock item not found for variant {reservation.VariantId}"
                };
            }

            var pickResult = stockItem.Pick(reservation.Quantity);
            if (pickResult.IsFailure)
            {
                return new ConsumeCartStockReservationsResponse
                {
                    Success = false,
                    ErrorMessage = pickResult.Errors.Count > 0
                        ? pickResult.Errors[0].Message
                        : "Insufficient stock"
                };
            }

            reservation.State = ReservationState.Fulfilled;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConsumeCartStockReservationsResponse
        {
            Success = true,
            ErrorMessage = null
        };
    }
}

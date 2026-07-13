using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Create;
/// <summary>Creates a new draft order from the provided request, maps domain data, and persists the entity to initialize the order lifecycle.</summary>
public static partial class CreateOrder
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Maps the request to an Order entity and saves it to the database, returning the created order details.</summary>
        /// <param name="command">The command containing the order creation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails due to constraint violation or concurrency conflict.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var request = command.Request;

            // Create: Map the request to a new Order entity with default identifiers.
            var result = request.MapToDomain(userId: Guid.Empty, storeId: request.StoreId);
            if (result.IsFailure)
                return result.Errors;

            var order = result.Value;

            dbContext.Set<Order>().Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(order.MapToDetail<Response>(), OrderResult.Success.Created(order.Id));
        }
    }
}

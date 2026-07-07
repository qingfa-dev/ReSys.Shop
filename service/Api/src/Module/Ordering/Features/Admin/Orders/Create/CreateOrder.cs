using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Create;
/// <summary>Handles CreateOrder feature.</summary>
public static partial class CreateOrder
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Map the request to a new Order entity.
            var result = request.MapToDomain(userId: Guid.Empty, storeId: Guid.Empty);
            if (result.IsFailure)
                return result.Failures;

            var order = result.Value;

            // Persist: Save the new entity to the database.
            dbContext.Set<Order>().Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return order.MapToDetail<Response>();
        }
    }
}

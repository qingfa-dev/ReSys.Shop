using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
namespace Module.Ordering.Features.Admin.Orders.AddLineItem;
/// <summary>Adds a new line item to an existing order, creating the line item entity and recalculating order totals.</summary>
public static partial class AddOrderLineItem
{
    public sealed record Command(Guid OrderId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Creates a line item from the request, adds it to the database, and recalculates the parent order's totals.</summary>
        /// <param name="command">The command containing the order ID and line item details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created line item response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Load the order first before creating any line item.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is null)
                return (Result<Response>)OrderResult.Errors.NotFound(command.OrderId);

            // Create: Build the line item from request data.
            var createResult = LineItemMethod.Create(command.OrderId, command.Request.VariantId, command.Request.Quantity, command.Request.Price);
            if (createResult.IsFailure)
                return createResult.Errors;

            var lineItem = createResult.Value;

            var addResult = order.AddLineItem(lineItem);
            if (addResult.IsFailure)
                return (Result<Response>)addResult.Errors;

            dbContext.Set<LineItem>().Add(addResult.Value);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(order.MapToDetail<Response>(), OrderResult.Success.ItemAdded(command.OrderId));
        }
    }
}

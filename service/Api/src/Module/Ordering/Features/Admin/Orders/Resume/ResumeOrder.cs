using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Events;
using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Admin.Orders.Resume;

/// <summary>Resumes a previously canceled order.</summary>
public static partial class ResumeOrder
{
    public class Response { public Guid Id { get; init; } public OrderStatus Status { get; init; } }
    public sealed record Command(Guid Id) : ICommand<Response>;
    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null) return (Result<Response>)OrderResult.Errors.NotFound(command.Id);

            var result = order.Resume();
            if (result.IsFailure) return (Result<Response>)result.Failures;

            // Reactivate: Resume associated shipments that were canceled.
            var shipments = await dbContext.Set<Shipment>()
                .Where(s => s.OrderId == order.Id && s.State == ShipmentState.Canceled)
                .ToListAsync(cancellationToken);

            foreach (var shipment in shipments)
            {
                shipment.Resume();
            }

            // Raise: Order resumed domain event.
            order.AddDomainEvent(new OrderResumedEvent(
                order.Id,
                order.Number,
                order.UserId!.Value,
                order.Email ?? string.Empty,
                DateTimeOffset.UtcNow));

            await dbContext.SaveChangesAsync(cancellationToken);
            return new Response { Id = order.Id, Status = order.Status };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Events;

namespace Module.Ordering.Features.Admin.Orders.ResendConfirmationEmail;

/// <summary>Resends the confirmation email for a placed order.</summary>
public static partial class ResendOrderConfirmationEmail
{
    public sealed record Command(Guid Id) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null) return OrderResult.Errors.NotFound(command.Id);

            if (order.Status != OrderStatus.Placed)
                return OrderResult.Errors.InvalidStatusTransition;

            order.AddDomainEvent(new OrderPlacedEvent(
                order.Id,
                order.Number,
                order.UserId!.Value,
                order.Email ?? string.Empty,
                order.Total,
                DateTimeOffset.UtcNow));

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}

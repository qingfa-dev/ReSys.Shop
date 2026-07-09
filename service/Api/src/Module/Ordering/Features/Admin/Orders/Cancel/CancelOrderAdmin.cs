using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Features.Admin.Orders.Cancel;
/// <summary>Handles CancelOrderAdmin feature.</summary>
public static partial class CancelOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the order exists.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            // Update: Cancel the order.
            var parsed = Guid.TryParse(currentUser.UserId, out var userId);
            var result = order.Cancel(userId);
            if (result.IsFailure)
                return result.Errors;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Notify: Send order canceled notification.
            await SendOrderCanceledNotificationAsync(order, cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }

        private async Task SendOrderCanceledNotificationAsync(Order order, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            var message = NotificationMessage.Create(
                NotificationUseCase.OrderCancelled,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            var result = await notificationService.SendAsync(message, ct);
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order canceled notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Description)));
            }
        }
    }
}

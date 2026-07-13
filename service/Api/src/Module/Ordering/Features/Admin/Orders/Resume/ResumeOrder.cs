using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Features.Admin.Orders.Resume;

/// <summary>Resumes a previously canceled order.</summary>
public static partial class ResumeOrder
{
    public sealed record Command(Guid Id) : ICommand<Response>;
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command, Response>
    {
        /// <summary>Restores a canceled order to its previous active state by invoking domain resume logic and sending a notification.</summary>
        /// <param name="command">The command containing the order ID to resume.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The resume response with updated status.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the order to resume.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null) return (Result<Response>)OrderResult.Errors.NotFound(command.Id);

            // Call: Invoke domain resume logic — transitions from Canceled to previous status.
            var result = order.Resume();
            if (result.IsFailure) return (Result<Response>)result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Notify: Send order confirmation to the customer's email.
            await SendOrderResumedNotificationAsync(order, cancellationToken);

            return Result<Response>.Ok(order.MapToDetail<Response>(), OrderResult.Success.Resumed(order.Id));
        }

        private async Task SendOrderResumedNotificationAsync(Order order, CancellationToken ct)
        {
            // Skip: No email on order — nothing to notify.
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            // Notify: Send order-resumed notification with order number and customer name.
            var message = NotificationMessage.Create(
                NotificationUseCase.OrderConfirmed,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            // Suppress: Notification failure must not block order resume — best-effort only.
            var result = await notificationService.SendAsync(message, ct);
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order resumed notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}
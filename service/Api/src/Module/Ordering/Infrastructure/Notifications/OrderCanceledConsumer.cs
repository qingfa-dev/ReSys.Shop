using BuildingBlocks.Mediators.Events;
using BuildingBlocks.Notifications.Models;
using BuildingBlocks.Notifications.Services;
using BuildingBlocks.Notifications.Templates;
using Microsoft.Extensions.Logging;
using Module.Ordering.Domain.Orders.Events;

namespace Module.Ordering.Infrastructure.Notifications;

public sealed class OrderCanceledConsumer(
    ILogger<OrderCanceledConsumer> logger,
    INotificationService notificationService)
    : DomainEventConsumer<OrderCanceledEvent>
{
    public override async Task Handle(OrderCanceledEvent notification, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Order canceled: {OrderId}, Number: {OrderNumber}, Customer: {CustomerId}, CanceledBy: {CanceledBy}",
                notification.OrderId, notification.OrderNumber, notification.CustomerId, notification.CanceledBy);
        }

        var message = NotificationMessage.Create(
            NotificationTemplateUseCase.SystemOrderFailed,
            NotificationRecipient.Create(notification.CustomerEmail, notification.OrderNumber),
            NotificationContext.Create(
                (NotificationParameterType.OrderId, notification.OrderNumber),
                (NotificationParameterType.OrderStatus, "Canceled"),
                (NotificationParameterType.UserFirstName, notification.CustomerEmail.Split('@')[0])));

        var result = await notificationService.SendAsync(message, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send order canceled notification for order {OrderId}: {Errors}",
                notification.OrderId,                 string.Join("; ", result.Failures.Select(f => f.Description)));
        }
    }
}

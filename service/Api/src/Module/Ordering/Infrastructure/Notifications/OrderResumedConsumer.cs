using BuildingBlocks.Mediators.Events;
using BuildingBlocks.Notifications.Models;
using BuildingBlocks.Notifications.Services;
using BuildingBlocks.Notifications.Templates;
using Microsoft.Extensions.Logging;
using Module.Ordering.Domain.Orders.Events;

namespace Module.Ordering.Infrastructure.Notifications;

public sealed class OrderResumedConsumer(
    ILogger<OrderResumedConsumer> logger,
    INotificationService notificationService)
    : DomainEventConsumer<OrderResumedEvent>
{
    public override async Task Handle(OrderResumedEvent notification, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Order resumed: {OrderId}, Number: {OrderNumber}, Customer: {CustomerId}",
                notification.OrderId, notification.OrderNumber, notification.CustomerId);
        }

        var message = NotificationMessage.Create(
            NotificationTemplateUseCase.SystemOrderConfirmation,
            NotificationRecipient.Create(notification.CustomerEmail, notification.OrderNumber),
            NotificationContext.Create(
                (NotificationParameterType.OrderId, notification.OrderNumber),
                (NotificationParameterType.OrderItems, "(see order details)"),
                (NotificationParameterType.UserFirstName, notification.CustomerEmail.Split('@')[0])));

        var result = await notificationService.SendAsync(message, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send order resumed notification for order {OrderId}: {Errors}",
                notification.OrderId,                 string.Join("; ", result.Failures.Select(f => f.Description)));
        }
    }
}

using Module.Ordering.Domain.Orders.Events;

namespace Module.Ordering.Infrastructure.Notifications;

public sealed class OrderPlacedConsumer(
    ILogger<OrderPlacedConsumer> logger,
    INotificationService notificationService)
    : DomainEventConsumer<OrderPlacedEvent>
{
    public override async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Order placed: {OrderId}, Number: {OrderNumber}, Customer: {CustomerId}, Email: {CustomerEmail}, Total: {Total}",
                notification.OrderId, notification.OrderNumber, notification.CustomerId, notification.CustomerEmail, notification.Total);
        }

        var message = NotificationMessage.Create(
            NotificationTemplateUseCase.SystemOrderConfirmation,
            NotificationRecipient.Create(notification.CustomerEmail, notification.OrderNumber),
            NotificationContext.Create(
                (NotificationParameterType.OrderId, notification.OrderNumber),
                (NotificationParameterType.OrderTotal, notification.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)),
                (NotificationParameterType.OrderItems, "(see order details)"),
                (NotificationParameterType.UserFirstName, notification.CustomerEmail.Split('@')[0])));

        var result = await notificationService.SendAsync(message, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send order confirmation notification for order {OrderId}: {Errors}",
                notification.OrderId,                 string.Join("; ", result.Failures.Select(f => f.Description)));
        }
    }
}
